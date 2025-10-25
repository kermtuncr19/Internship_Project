using System.ComponentModel.DataAnnotations;

namespace Entities.Models;

public class Product
{
    public int ProductId { get; set; }
    public String? ProductName { get; set; } = String.Empty;
    public decimal Price { get; set; }
    public String? Summary { get; set; } = String.Empty;
    public String? ImageUrl { get; set; }
    //public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public int? CategoryId { get; set; }//foreign key
    public Category? Category { get; set; }//navigation property
    public bool ShowCase { get; set; }
    public bool RequiresSize { get; set; } = false;
    public string? SizeOptionsCsv { get; set; }

}
