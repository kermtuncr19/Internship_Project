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
        public int? CategoryId { get; init; }
       
    }
}