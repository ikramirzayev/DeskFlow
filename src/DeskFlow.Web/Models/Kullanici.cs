using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace DeskFlow.Web.Models;

public class Kullanici : IdentityUser
{
    [Required]
    [StringLength(50)]
    public string Ad { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Soyad { get; set; } = string.Empty;

    [StringLength(20)]
    public string? OgrenciNo { get; set; }

    public DateTime KayitTarihi { get; set; } = DateTime.UtcNow;

    public List<Rezervasyon> Rezervasyonlar { get; set; } = new();
}
