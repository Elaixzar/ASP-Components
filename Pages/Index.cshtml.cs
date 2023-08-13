using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ASP_Components.Pages
{
    public class IndexModel : PageModel
    {

        //Local references
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}