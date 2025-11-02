using System.ComponentModel.DataAnnotations;

namespace Entities.Models;

public class Product
{
    public int ProductId { get; set; }
    public string? ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Summary { get; set; } = string.Empty;

    // ⚠️ Geriye dönük uyumluluk için tutuyoruz (eski ürünler için)
    public string? ImageUrl { get; set; }

    // ✅ Yeni çoklu fotoğraf sistemi
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
    public bool ShowCase { get; set; }
    public bool RequiresSize { get; set; } = false;
    public string? SizeOptionsCsv { get; set; }

    // Product.cs modeline ekle:

    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    // Ortalama puan için computed property (isteğe bağlı)
    public double AverageRating
    {
        get
        {
            if (Reviews == null || !Reviews.Any(r => r.IsApproved))
                return 0;
            return Reviews.Where(r => r.IsApproved).Average(r => r.Rating);
        }
    }

    public int ReviewCount
    {
        get
        {
            if (Reviews == null)
                return 0;
            return Reviews.Count(r => r.IsApproved);
        }
    }
}