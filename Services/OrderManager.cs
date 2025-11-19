using Entities.Models;
using Repositories.Contracts;
using Services.Contracts;

namespace Services
{
    public class OrderManager : IOrderService
    {
        private readonly IRepositoryManager _manager;

        public OrderManager(IRepositoryManager manager)
        {
            _manager = manager;
        }

        public IQueryable<Order> Orders => _manager.Order.Orders;

        public int NumberOfInProcess => _manager.Order.NumberOfInProcess;

        public void Complete(int id)
        {
            _manager.Order.Complete(id);
            _manager.Save();
        }

        public void Delete(int id)
        {
            var order = _manager.Order.GetOneOrder(id);
            if (order is null) return;

            _manager.Order.Delete(order);
            _manager.Save();
        }

        public Order? GetOneOrder(int id)
        {
            return _manager.Order.GetOneOrder(id);
        }

        public void SaveOrder(Order order)
        {
            // Eğer sipariş iptal ediliyorsa ve önceden onaylanmışsa stokları geri yükle
            var existingOrder = _manager.Order.GetOneOrder(order.OrderId);

            if (existingOrder != null &&
                order.Cancelled &&
                !existingOrder.Cancelled &&
                existingOrder.Shipped)
            {
                // Sipariş iptal ediliyor ve daha önce onaylanmıştı
                // Stokları geri yükle
                foreach (var line in existingOrder.Lines)
                {
                    try
                    {
                        var stock = _manager.ProductStock.GetStockByProductAndSize(
                            line.ProductId,
                            line.Size,
                            true
                        );

                        if (stock != null)
                        {
                            stock.Quantity += line.Quantity;
                            stock.UpdatedAt = DateTime.UtcNow;
                        }
                        else
                        {
                            // Stok kaydı yoksa oluştur
                            var newStock = new ProductStock
                            {
                                ProductId = line.ProductId,
                                Size = line.Size,
                                Quantity = line.Quantity,
                                CreatedAt = DateTime.UtcNow
                            };
                            _manager.ProductStock.CreateStock(newStock);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error but don't fail the cancellation
                        System.Diagnostics.Debug.WriteLine($"Stok geri yükleme hatası: {ex.Message}");
                    }
                }

                _manager.Save();
            }

            _manager.Order.SaveOrder(order);
        }

        public Task<List<Order>> GetMyOrdersAsync(string userId)
      => _manager.Order.GetByUserAsync(userId);

        public Task<Order?> GetMyOrderAsync(string userId, int id)
            => _manager.Order.GetOneAsync(id, userId);
    }
}