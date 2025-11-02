using System.ComponentModel.DataAnnotations;

namespace StoreApp.Models
{
    public class ReviewViewModel
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductImageUrl { get; set; }
        public int OrderId { get; set; }
        
        [Required(ErrorMessage = "Lütfen puan verin.")]
        [Range(1, 5, ErrorMessage = "Puan 1-5 arasında olmalıdır.")]
        public int Rating { get; set; }
        
        [MaxLength(1000, ErrorMessage = "Yorum en fazla 1000 karakter olabilir.")]
        public string? Comment { get; set; }
        public bool ShowFullName { get; set; } = false;
    }
    
    public class ProductReviewStatusViewModel
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductImageUrl { get; set; }
        public int OrderId { get; set; }
        public bool HasReview { get; set; }
        public int? ExistingRating { get; set; }
        public string? ExistingComment { get; set; }
    }
}