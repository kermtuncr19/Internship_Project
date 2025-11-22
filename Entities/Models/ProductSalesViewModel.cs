namespace Entities.Models
{
    public class ProductSalesViewModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public int SalesCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}