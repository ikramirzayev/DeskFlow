using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;


namespace DeskFlow.Web.Pages
{
    public class KayitOlModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Ad zorunludur.")]
            public string Ad { get; set; } = string.Empty;

            [Required(ErrorMessage = "Soyad zorunludur.")]
            public string Soyad { get; set; } = string.Empty;

            [Required(ErrorMessage = "Öğrenci numarası zorunludur.")]
            [StringLength(8, MinimumLength = 8, ErrorMessage = "Öğrenci numarası 8 karakter olmalıdır.")]

            public string OgrenciNo { get; set; } = string.Empty;

            [Required(ErrorMessage = "E-posta zorunludur.")]
            [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Şifre zorunludur.")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
            [DataType(DataType.Password)]
            public string Sifre { get; set; } = string.Empty;

            [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
            [DataType(DataType.Password)]
            [Compare("Sifre", ErrorMessage = "Şifreler eşleşmiyor.")]
            public string SifreTekrar { get; set; } = string.Empty;
        }
        public void OnGet() { }

        public IActionResult OnPost() 
        {
            if (!ModelState.IsValid) return Page();
            //kullanıcı oluşturma
            return RedirectToPage("/GirisYap");
        }
    }
}
