using ASP_Components.Models;
using ASP_Components.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ASP_Components.Middleware
{

    /// <summary>
    /// A middleware that redirects requests based on a collection of <see cref="RedirectData"/> objects.
    /// </summary>
    [Route("Redirect")]
    [ApiController]
    public class RedirectMiddleware
    {
        //Local references to services
        public IRedirectService? RedirectService;
        private readonly ILogger<RedirectMiddleware>? _logger;
        private readonly RequestDelegate _next;

        //Local cache params
        private volatile IEnumerable<RedirectData>? _redirectData;
        private DateTime _lastFetchedTime = DateTime.MinValue;
        private readonly TimeSpan _cacheDuration;
        private readonly object _updateLock = new object();

        /// <summary>
        /// Initializes a new instance of the RedirectMiddleware class.
        /// </summary>
        /// <param name="next">The delegate representing the next middleware in the pipeline.</param>
        /// <param name="logger">An ILogger instance for logging events and messages.</param>
        /// <param name="redirectService">An IRedirectService implementation for handling redirections.</param>
        /// <param name="configuration">An IConfiguration instance providing access to application configuration settings.</param>
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

        /// <summary>
        /// Invokes the middleware to handle redirection logic based on the requested path.
        /// </summary>
        /// <param name="context">The HttpContext for the current request.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Invoke(HttpContext context)
        {

            //Check if cache is stale
            if (_redirectData == null || DateTime.UtcNow - _lastFetchedTime > _cacheDuration)
            {
                //Fire the update -> DOES NOT WAIT FOR UPDATE TO COMPLETE.
                //This is intentional to avoid blocking the request.
                //The next request should have the updated data.
                //If this behavior is not desired, then the update will need to be on a timer and removed from the invoke method.
                UpdateFromSourceAsync();
            }

            //Check requested path
            var pathValue = context.Request.Path.Value;
            var sanitizedPath = pathValue?.Replace("//", "/");

            try
            {

                if (!string.IsNullOrEmpty(sanitizedPath))
                {

                    ///Check if any of the redirects apply
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

                    //If a redirect is found, apply it
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
            }
            catch (Exception ex)
            {
                _logger?.LogError("RedirectMiddleware: Error processing request for " + sanitizedPath + ". " + ex.Message);
            }

            await _next(context);
        }

        /// <summary>
        /// This method updates the local cache from the source.  
        /// This method is asynchronous and does not block the request.
        /// It is public to allow for testing and manual updates if desired.
        /// </summary>
        public async void UpdateFromSourceAsync()
        {

            //Local Cache
            IEnumerable<RedirectData>? newRedirectData = null;

            //Check for nulls
            try
            {

                //Ensure RedirectService is not null
                if (RedirectService == null)
                {
                    _logger?.LogError(this.GetType().Name + " cannot get RedirectService, RedirectService is null.");
                }
                else
                {

                    //Get RedirectData
                    newRedirectData = await RedirectService.GetRedirectDataAsync();

                    //Ensure RedirectData is not null
                    if (_redirectData == null)
                    {
                        _logger?.LogError(this.GetType().Name + " cannot get RedirectData, RedirectData is null.");
                    }
                    else
                    {

                        //Main Update
                        var timeSinceLastFetch = _lastFetchedTime == DateTime.MinValue ? "" : $" {((DateTime.UtcNow - _lastFetchedTime)).ToString(@"hh\:mm\:ss")} since previous fetch.";
                        _logger?.LogInformation($"RedirectData Updated.{timeSinceLastFetch}");
                        _logger?.LogInformation(this.GetType().Name + $" RedirectData: {JsonSerializer.Serialize(_redirectData)}");
                    }
                }

            }
            catch (Exception ex)
            {
                _logger?.LogError(this.GetType().Name + " cannot get RedirectData, exception: " + ex.Message);
            }

            //Update local cache. 
            //This is done outside of the try/catch to ensure the cache is updated even if the source is unavailable.
            //This is also threadlocked to ensure the cache is not updated by multiple threads at the same time.
            lock (_updateLock)
            {
                _redirectData = newRedirectData;
                _lastFetchedTime = DateTime.UtcNow;
            }

        }
    }

}
