using System.ComponentModel.DataAnnotations;

namespace Entities.Dto
{
    public record ProductDto
    {
        public int ProductId { get; init; }//init ile nesnenin değişmeyeceğinin garantisini veriyoruz.
        [Required(ErrorMessage = "Product name is required!")]
        public String? ProductName { get; init; } = String.Empty;
        [Required(ErrorMessage = "Price is required!")]
        public decimal Price { get; init; }
        public String? Summary { get; init; } = String.Empty;
        public String? ImageUrl { get; set; }
        public int? CategoryId { get; init; }
        
        public bool RequiresSize { get; init; }
        public string? SizeOptionsCsv { get; init; }

    }
}