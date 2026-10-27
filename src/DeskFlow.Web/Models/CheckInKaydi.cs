using System.ComponentModel.DataAnnotations;

namespace DeskFlow.Web.Models;

public class CheckInKaydi
{
    public int Id { get; set; }

    [Required]
    public int RezervasyonId { get; set; }

    public DateTime CheckInZamani { get; set; }

    // Check-out henüz yapılmadıysa boş kalır.
    public DateTime? CheckOutZamani { get; set; }

    [Required]
    [StringLength(20)]
    public string IslemTuru { get; set; } = "CheckIn";

    public Rezervasyon Rezervasyon { get; set; } = null!;
}
