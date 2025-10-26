using System.ComponentModel.DataAnnotations;

namespace Entities.Models;

public class ProductImage
{
    public int ProductImageId { get; set; }
    
    [Required]
    public string ImageUrl { get; set; } = string.Empty;
    
    public int DisplayOrder { get; set; } // Fotoğrafların sıralaması için
    
    public bool IsMain { get; set; } = false; // Ana fotoğraf işareti
    
    // Foreign Key
    public int ProductId { get; set; }
    
    // Navigation Property
    public Product? Product { get; set; }
}