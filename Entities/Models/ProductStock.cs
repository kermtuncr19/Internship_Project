using System.ComponentModel.DataAnnotations;

namespace Entities.Models
{
    public class ProductStock
    {
        public int ProductStockId { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        
        // Beden bilgisi (bedensiz ürünler için null olacak)
        public string? Size { get; set; }
        
        // Stok miktarı
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}