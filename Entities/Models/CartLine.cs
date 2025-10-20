namespace Entities.Models
{
    public class CartLine
    {
        public int CartLineId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public Order Order { get; set; } = default!;
        public Product Product { get; set; } = default!;
        public int Quantity { get; set; }
        public string? Size { get; set; }
        public decimal UnitPrice { get; set; }
    }
}