using ASP_Components.Models;
using System.Text.Json;

namespace ASP_Components.Services
{

    /// <summary>
    /// This is an interface for the Redirect Service required for the Redirect Middleware
    /// </summary>
    public interface IRedirectService
    {
        Task<IEnumerable<RedirectData>> GetRedirectDataAsync();
    }

    /// <summary>
    /// This is a service for the Redirect Middleware.  This retrieves the redirect data from a file.
    /// In a production environement, this would be replaced with a service that retrieves the data from a database.
    /// </summary>
    public class RedirectService : IRedirectService
    {

        //Local references
        public IWebHostEnvironment? WebHostEnvironment { get; }
        private readonly ILogger<RedirectService>? _logger;

        //Constructor
        public RedirectService(IWebHostEnvironment webHostEnvironment, ILogger<RedirectService> logger)
        {
            WebHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        /// <summary>
        /// Asynchronously retrieves a collection of redirect data.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.
        /// The task result contains a collection of <see cref="RedirectData"/> objects.</returns>
        public async Task<IEnumerable<RedirectData>> GetRedirectDataAsync()
        {
            using (var jsonFileReader = File.OpenText(MockRedirectDataFileName()))
            {

                var fileContent = await jsonFileReader.ReadToEndAsync();
                var returnData = JsonSerializer.Deserialize<RedirectData[]>(fileContent,
                                                  new JsonSerializerOptions
                                                  {
                                                      PropertyNameCaseInsensitive = true
                                                  });

                if (returnData == null)
                {
                    _logger?.LogError("Cannot get RedirectData, returnData is null.");
                    return new List<RedirectData>();
                }
                else
                {
                    return returnData;
                }


            }
        }

        //File Path
        private string MockRedirectDataFileName()
        {
            if (WebHostEnvironment == null)
            {
                _logger?.LogError("Cannot get MockRedirectDataFileName, WebHostEnvironment is null.");
                return "";
            }
            return Path.Combine(WebHostEnvironment.WebRootPath, "data", "SampleServiceResult.json");
        }


    }
}
