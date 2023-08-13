using ASP_Components.Models;
using ASP_Components.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ASP_Components.Middleware
{
    [Route("Redirect")]
    [ApiController]
    public class RedirectMiddleware
    {
        //Public References
        public IRedirectService? RedirectService;

        //Local references
        private readonly ILogger<RedirectMiddleware>? _logger;
        private readonly RequestDelegate _next;
        private DateTime _lastFetchedTime = DateTime.MinValue;
        private readonly TimeSpan _cacheDuration;
        private volatile IEnumerable<RedirectData>? _redirectData;
        private readonly object _updateLock = new object();


        //Constructor
        public RedirectMiddleware(
            RequestDelegate next,
            ILogger<RedirectMiddleware> logger,
            IRedirectService redirectService,
            IConfiguration configuration)
        {

            //Update local references
            _logger = logger;
            _next = next;
            RedirectService = redirectService;

            //Check for config
            _cacheDuration = TimeSpan.FromMinutes(double.Parse(configuration?["RedirectCacheLifespan_Minutes"] ?? "5"));

            //Update from source
            UpdateFromSourceAsync();
        }
        public async Task Invoke(HttpContext context)
        {
            if (_redirectData == null || DateTime.UtcNow - _lastFetchedTime > _cacheDuration)
            {
                // Use a separate task to update the cache
                UpdateFromSourceAsync();
            }

            //Check requested path
            var pathValue = context.Request.Path.Value;
            var sanitizedPath = pathValue?.Replace("//", "/");

            if (!string.IsNullOrEmpty(sanitizedPath))
            {

                ///Check if any of the redirect apply
                var redirectConfig = _redirectData?.FirstOrDefault((rc) =>
                {
                    if (rc.RedirectUrl == null) return false;
                    if (rc.UseRelative)
                    {
                        if (rc.RedirectUrl.Length > sanitizedPath.Length) return false;
                        return sanitizedPath.Substring(0, rc.RedirectUrl.Length).Equals(rc.RedirectUrl, StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        return rc.RedirectUrl.Equals(sanitizedPath, StringComparison.OrdinalIgnoreCase);
                    }
                });

                //If a redirect is found, redirect
                if (redirectConfig != null && redirectConfig.RedirectUrl != null)
                {
                    var targetUrl = redirectConfig.UseRelative
                        ? $"{sanitizedPath.Replace(redirectConfig.RedirectUrl, redirectConfig.TargetUrl, StringComparison.OrdinalIgnoreCase)}"
                        : redirectConfig.TargetUrl;

                    if (string.IsNullOrEmpty(targetUrl))
                    {
                        _logger?.LogError("RedirectMiddleware: targetUrl is null or empty.");
                    }
                    else
                    {
                        context.Response.Redirect(targetUrl, redirectConfig.RedirectType == 301);
                        _logger?.LogInformation("RedirectMiddleware: Redirected " + sanitizedPath + " to " + targetUrl + ".");
                        return;
                    }

                }

            }

            await _next(context);
        }

        //Methods
        public async void UpdateFromSourceAsync()
        {
            IEnumerable<RedirectData>? newRedirectData = null;

            //Check for nulls
            try
            {

                if (RedirectService == null)
                {
                    _logger?.LogError(this.GetType().Name + " cannot get RedirectService, RedirectService is null.");
                }
                else
                {

                    newRedirectData = await RedirectService.GetRedirectDataAsync();
                    if (_redirectData == null)
                    {
                        _logger?.LogError(this.GetType().Name + " cannot get RedirectData, RedirectData is null.");
                    }
                    else
                    {
                        var timeSinceLastFetch = _lastFetchedTime == DateTime.MinValue ? "" : $" {((DateTime.UtcNow - _lastFetchedTime)).ToString(@"hh\:mm\:ss")} since previous fetch.";
                        _logger?.LogInformation($"RedirectData Updated.{timeSinceLastFetch}");
                        _logger?.LogInformation(this.GetType().Name + $" RedirectData: {JsonSerializer.Serialize(_redirectData)}");
                    }
                }

                lock (_updateLock)
                {
                    _redirectData = newRedirectData;
                    _lastFetchedTime = DateTime.UtcNow;
                }

            }
            catch (Exception ex)
            {
                _logger?.LogError(this.GetType().Name + " cannot get RedirectData, exception: " + ex.Message);
            }

        }
    }

}
