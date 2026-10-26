using DeskFlow.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeskFlow.Web.Pages
{
    public class CikisYapModel : PageModel
    {
        private readonly SignInManager<Kullanici> _signInManager;

        public CikisYapModel(SignInManager<Kullanici> signInManager)
        {
            _signInManager = signInManager;
        }

        public IActionResult OnGet()
        {
            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _signInManager.SignOutAsync();
            return RedirectToPage("/Index");
        }
    }
}
