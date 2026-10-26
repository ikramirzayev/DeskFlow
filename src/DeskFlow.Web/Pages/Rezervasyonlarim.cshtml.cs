using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeskFlow.Web.Pages
{
    [Authorize]
    public class RezervasyonlarımModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
