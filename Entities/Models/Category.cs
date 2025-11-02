namespace Entities.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(50)]
        public String? CategoryName { get; set; } = String.Empty;
        //collection navigation property
        public ICollection<Product> Products { get; set; }
    }
}