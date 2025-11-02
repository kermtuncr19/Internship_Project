using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Entities.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        
        [Required]
        public string? UserId { get; set; }
        public IdentityUser? User { get; set; }
        
        [Required]
        public int OrderId { get; set; }
        public Order? Order { get; set; }
        
        [Required]
        [Range(1, 5, ErrorMessage = "Puan 1-5 arasında olmalıdır.")]
        public int Rating { get; set; }
        
        [MaxLength(1000)]
        public string? Comment { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsApproved { get; set; } = false;
        
        // Kullanıcı adı görünürlüğü
        public bool ShowFullName { get; set; } = false; // true: Tam ad, false: İlk iki harf + ***
    }
}