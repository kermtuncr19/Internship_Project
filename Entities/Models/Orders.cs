using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Entities.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public ICollection<CartLine> Lines { get; set; } = new List<CartLine>();

        [Required(ErrorMessage = "İsim Gerekli!")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Şehir Bilgisi Gerekli!")]
        public string? City { get; set; }

        [Required(ErrorMessage = "İlçe Bilgisi Gerekli!")]
        public string? District { get; set; }

        [Required(ErrorMessage = "Mahalle Bilgisi Gerekli!")]
        public string? Neighborhood { get; set; }

        [Required(ErrorMessage = "Açık Adres Gerekli!")]
        [Display(Name = "Açık Adres")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Telefon numarası gerekli!")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası girin.")]
        [MaxLength(20)]
        [Display(Name = "Telefon")]
        [RegularExpression(@"^(\+90\s?)?0?(5\d{2})\s?\d{3}\s?\d{2}\s?\d{2}$", ErrorMessage = "Telefon 05xx xxx xx xx formatında olmalı.")]
        public string? PhoneNumber { get; set; }

        public bool GiftWrap { get; set; }
        public bool Shipped { get; set; }
        public DateTime? ShippedAt { get; set; }

        public bool Cancelled { get; set; } = false;
        public DateTime? CancelledAt { get; set; }

        // Kargo takip alanları
        public bool Preparing { get; set; }
        public DateTime? PreparingAt { get; set; }

        public bool InTransit { get; set; }
        public DateTime? InTransitAt { get; set; }

        public bool Delivered { get; set; }
        public DateTime? DeliveredAt { get; set; }

        public DateTime OrderedAt { get; set; } = DateTime.UtcNow;

        public int? Installment { get; set; }

        public string? UserId { get; set; }
        public IdentityUser? User { get; set; }

        public bool CancelledByUser { get; set; }
        public DateTime? CancelledByUserAt { get; set; }
        public string? CancellationReason { get; set; }

        public List<ReturnRequest> ReturnRequests { get; set; } = new();
    }
}