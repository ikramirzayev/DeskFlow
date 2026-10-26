using System.ComponentModel.DataAnnotations;
using DeskFlow.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeskFlow.Web.Pages
{
    public class KayitOlModel : PageModel
    {
        private readonly UserManager<Kullanici> _userManager;
        private readonly SignInManager<Kullanici> _signInManager;

        public KayitOlModel(UserManager<Kullanici> userManager, SignInManager<Kullanici> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Ad zorunludur.")]
            [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir.")]
            [Display(Name = "Ad")]
            public string Ad { get; set; } = string.Empty;

            [Required(ErrorMessage = "Soyad zorunludur.")]
            [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir.")]
            [Display(Name = "Soyad")]
            public string Soyad { get; set; } = string.Empty;

            [Required(ErrorMessage = "Öğrenci numarası zorunludur.")]
            [RegularExpression(@"^\d{8,11}$", ErrorMessage = "Öğrenci numarası 8–11 haneli bir sayı olmalıdır.")]
            [Display(Name = "Öğrenci No")]
            public string OgrenciNo { get; set; } = string.Empty;

            [Required(ErrorMessage = "E-posta zorunludur.")]
            [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
            [Display(Name = "E-posta")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Şifre zorunludur.")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
            [DataType(DataType.Password)]
            [Display(Name = "Şifre")]
            public string Sifre { get; set; } = string.Empty;

            [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
            [DataType(DataType.Password)]
            [Compare(nameof(Sifre), ErrorMessage = "Şifreler eşleşmiyor.")]
            [Display(Name = "Şifre (tekrar)")]
            public string SifreTekrar { get; set; } = string.Empty;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var kullanici = new Kullanici
            {
                UserName = Input.Email,
                Email = Input.Email,
                Ad = Input.Ad,
                Soyad = Input.Soyad,
                OgrenciNo = Input.OgrenciNo
            };

            var sonuc = await _userManager.CreateAsync(kullanici, Input.Sifre);
            if (!sonuc.Succeeded)
            {
                foreach (var hata in sonuc.Errors)
                {
                    ModelState.AddModelError(string.Empty, hata.Description);
                }
                return Page();
            }

            await _userManager.AddToRoleAsync(kullanici, Roller.Ogrenci);
            await _signInManager.SignInAsync(kullanici, isPersistent: false);

            return RedirectToPage("/Index");
        }
    }
}
