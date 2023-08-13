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
        private IEnumerable<RedirectData>? _redirectData { get; set; }

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
            _cacheDuration = TimeSpan.FromMinutes(double.Parse(configuration["RedirectCacheLifespan_Minutes"] ?? "5"));

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
                    return rc.RedirectUrl.Equals(sanitizedPath, StringComparison.OrdinalIgnoreCase);
                });

                //If a redirect is found, redirect
                if (redirectConfig != null)
                {
                    var targetUrl = redirectConfig.UseRelative
                        ? $"{context.Request.PathBase}{redirectConfig.TargetUrl}"
                        : redirectConfig.TargetUrl;

                    context.Response.Redirect(targetUrl, redirectConfig.RedirectType == 301);
                    return;
                }

            }

            await _next(context);
        }

        //Methods
        private async void UpdateFromSourceAsync()
        {

            //Check for nulls
            if (RedirectService == null)
            {
                _logger?.LogError(this.GetType().Name + " cannot get RedirectService, RedirectService is null.");
            }
            else
            {
                _redirectData = await RedirectService.GetRedirectDataAsync();
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

            _lastFetchedTime = DateTime.UtcNow;
        }
    }

}
