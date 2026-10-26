using System.ComponentModel.DataAnnotations;
using DeskFlow.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeskFlow.Web.Pages
{
    public class GirisYapModel : PageModel
    {
        private readonly SignInManager<Kullanici> _signInManager;

        public GirisYapModel(SignInManager<Kullanici> signInManager)
        {
            _signInManager = signInManager;
        }
        [BindProperty]
        public InputModel Input { get; set; } = new();
        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "E-posta zorunludur.")]
            [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
            [Display(Name = "E-posta")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Şifre zorunludur.")]
            [DataType(DataType.Password)]
            [Display(Name = "Şifre")]
            public string Sifre { get; set; } = string.Empty;

            [Display(Name = "Beni hatırla")]
            public bool BeniHatirla { get; set; }
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var sonuc = await _signInManager.PasswordSignInAsync(
                Input.Email, Input.Sifre, Input.BeniHatirla, lockoutOnFailure: true);

            if (sonuc.Succeeded)
            {
                return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/");
            }

            if (sonuc.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Çok fazla hatalı deneme yapıldı. Lütfen 5 dakika sonra tekrar deneyin.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
            }

            return Page();
        }
    }
}
