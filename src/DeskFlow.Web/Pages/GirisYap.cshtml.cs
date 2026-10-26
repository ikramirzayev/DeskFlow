using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeskFlow.Web.Pages
{
    public class GirisYapModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "E-posta zorunludur.")]
            [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Şifre zorunludur.")]
            [DataType(DataType.Password)]
            public string Sifre { get; set; } = string.Empty;

            public bool BeniHatirla {  get; set; }
        }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();
            //giriş işlemi

            return RedirectToPage("/Index");
        }

    }
}
