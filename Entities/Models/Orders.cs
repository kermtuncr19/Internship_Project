using System.ComponentModel.DataAnnotations;

namespace Entities.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public ICollection<CartLine> Lines { get; set; } = new List<CartLine>();

        [Required(ErrorMessage = "İsim Gerekli!")]
        public string? Name { get; set; }

        // ŞEHİR / İLÇE / MAHALLE / AÇIK ADRES
        [Required(ErrorMessage = "Şehir Bilgisi Gerekli!")]
        public string? City { get; set; }

        [Required(ErrorMessage = "İlçe Bilgisi Gerekli!")]
        public string? District { get; set; }   // yeni

        [Required(ErrorMessage = "Mahalle Bilgisi Gerekli!")]
        public string? Neighborhood { get; set; } // Line1'den rename

        [Required(ErrorMessage = "Açık Adres Gerekli!")]
        [Display(Name = "Açık Adres")]
        public string? Address { get; set; }    // yeni (cadde/sokak yerine tek alan)
        [Required(ErrorMessage = "Telefon numarası gerekli!")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası girin.")]
        [MaxLength(20)]
        [Display(Name = "Telefon")]
        [RegularExpression(@"^(\+90\s?)?0?(5\d{2})\s?\d{3}\s?\d{2}\s?\d{2}$",
 ErrorMessage = "Telefon 05xx xxx xx xx formatında olmalı.")]
        public string? PhoneNumber { get; set; }

        public bool GiftWrap { get; set; }
        public bool Shipped { get; set; }
        public bool Cancelled { get; set; } = false;

        public DateTime OrderedAt { get; set; } = DateTime.UtcNow;
    }
}
