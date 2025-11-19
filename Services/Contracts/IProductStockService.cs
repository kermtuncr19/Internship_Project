using Entities.Models;

namespace Services.Contracts
{
    public interface IProductStockService
    {
        IEnumerable<ProductStock> GetStocksByProductId(int productId);
        ProductStock? GetStockByProductAndSize(int productId, string? size);
        void CreateOrUpdateStock(int productId, string? size, int quantity);
        void DeleteStock(int stockId);
        bool IsInStock(int productId, string? size, int quantity = 1);
        void DecreaseStock(int productId, string? size, int quantity);
    }
}