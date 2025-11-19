using Entities.Models;
using Repositories.Contracts;
using Services.Contracts;

namespace Services
{
    public class ProductStockManager : IProductStockService
    {
        private readonly IRepositoryManager _manager;

        public ProductStockManager(IRepositoryManager manager)
        {
            _manager = manager;
        }

        public IEnumerable<ProductStock> GetStocksByProductId(int productId)
        {
            return _manager.ProductStock
                .GetStocksByProductId(productId, false)
                .ToList();
        }

        public ProductStock? GetStockByProductAndSize(int productId, string? size)
        {
            return _manager.ProductStock
                .GetStockByProductAndSize(productId, size, false);
        }

        public void CreateOrUpdateStock(int productId, string? size, int quantity)
        {
            var existingStock = _manager.ProductStock
                .GetStockByProductAndSize(productId, size, true);

            if (existingStock != null)
            {
                // Güncelle
                existingStock.Quantity = quantity;
                existingStock.UpdatedAt = DateTime.UtcNow;
                _manager.Save();
            }
            else
            {
                // Yeni oluştur
                var newStock = new ProductStock
                {
                    ProductId = productId,
                    Size = size,
                    Quantity = quantity,
                    CreatedAt = DateTime.UtcNow
                };
                _manager.ProductStock.CreateStock(newStock);
                _manager.Save();
            }
        }

        public void DeleteStock(int stockId)
        {
            var stock = _manager.ProductStock
                .FindByCondition(s => s.ProductStockId == stockId, false);
                
            if (stock != null)
            {
                _manager.ProductStock.DeleteStock(stock);
                _manager.Save();
            }
        }

        public bool IsInStock(int productId, string? size, int quantity = 1)
        {
            var stock = GetStockByProductAndSize(productId, size);
            return stock != null && stock.Quantity >= quantity;
        }

        public void DecreaseStock(int productId, string? size, int quantity)
        {
            var stock = _manager.ProductStock
                .GetStockByProductAndSize(productId, size, true);

            if (stock == null)
                throw new Exception("Stok bulunamadı.");

            if (stock.Quantity < quantity)
                throw new Exception("Yetersiz stok.");

            stock.Quantity -= quantity;
            stock.UpdatedAt = DateTime.UtcNow;
            _manager.Save();
        }
    }
}