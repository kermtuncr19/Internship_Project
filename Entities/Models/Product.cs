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
}