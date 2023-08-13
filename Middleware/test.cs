//using ASP_Components.Services;

//public class RedirectMiddleware
//{
//    private readonly RequestDelegate _next;
//    private readonly ILogger<RedirectMiddleware> _logger;
//    private readonly IRedirectService _redirectService;
//    private List<RedirectConfig> _cache;
//    private DateTime _lastFetchTime;
//    private readonly TimeSpan _cacheDuration;

//    public RedirectMiddleware(
//        RequestDelegate next,
//        ILogger<RedirectMiddleware> logger,
//        IRedirectService redirectService,
//        IConfiguration configuration)
//    {
//        _next = next;
//        _logger = logger;
//        _redirectService = redirectService;
//        _cacheDuration = TimeSpan.FromMinutes(int.Parse(configuration["CacheDuration"] ?? "5"));
//    }

//    public async Task Invoke(HttpContext context)
//    {
//        if (_cache == null || DateTime.UtcNow - _lastFetchTime > _cacheDuration)
//        {
//            // Use a separate task to update the cache
//            _ = UpdateCacheAsync();
//        }

//        var redirectConfig = _cache?.FirstOrDefault(rc => rc.RedirectUrl.Equals(context.Request.Path, StringComparison.OrdinalIgnoreCase));

//        if (redirectConfig != null)
//        {
//            var targetUrl = redirectConfig.UseRelative
//                ? $"{context.Request.PathBase}{redirectConfig.TargetUrl}"
//                : redirectConfig.TargetUrl;

//            context.Response.Redirect(targetUrl, redirectConfig.RedirectType == 301);
//            return;
//        }

//        await _next(context);
//    }










//    private async Task UpdateCacheAsync()
//    {
//        try
//        {
//            _cache = await _redirectService.GetRedirectsAsync();
//            _lastFetchTime = DateTime.UtcNow;
//            _logger.LogInformation("Cache updated successfully.");
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error fetching redirects from the API.");
//        }
//    }
//}
