using System.ComponentModel.DataAnnotations;

namespace Entities.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public ICollection<CartLine> Lines { get; set; } = new List<CartLine>();

        [Required(ErrorMessage = "İsim Gerekli!")]
        public String? Name { get; set; }

        [Required(ErrorMessage = "Mahalle Bilgisi Gerekli!")]
        public String? Line1 { get; set; }
        [Required(ErrorMessage = "Cadde Bilgisi Gerekli!")]
        public String? Line2 { get; set; }
        [Required(ErrorMessage = "Sokak Bilgisi Gerekli!")]
        public String? Line3 { get; set; }
        [Required(ErrorMessage = "Şehir Bilgisi Gerekli!")]
        public String? City { get; set; }
        public bool GiftWrap { get; set; }
        public bool Shipped { get; set; }
        public DateTime OrderedAt { get; set; } = DateTime.Now;

    }
    
}