using ASP_Components.Models;
using System.Text.Json;

namespace ASP_Components.Services
{

    public interface IRedirectService
    {
        Task<IEnumerable<RedirectData>> GetRedirectDataAsync();
    }

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

        //Methods
        public string MockRedirectDataFileName()
        {
            if (WebHostEnvironment == null)
            {
                _logger?.LogError("Cannot get MockRedirectDataFileName, WebHostEnvironment is null.");
                return "";
            }
            return Path.Combine(WebHostEnvironment.WebRootPath, "data", "SampleServiceResult.json");
        }

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


    }
}
