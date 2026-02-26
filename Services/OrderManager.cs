using Entities.Models;
using Microsoft.EntityFrameworkCore;
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
            System.Diagnostics.Debug.WriteLine($"═══════════════════════════════════════");
            System.Diagnostics.Debug.WriteLine($"🔍 SaveOrder çağrıldı: OrderId={order.OrderId}");
            System.Diagnostics.Debug.WriteLine($"🔍 order.Cancelled={order.Cancelled}");
            System.Diagnostics.Debug.WriteLine($"🔍 order.Lines?.Count={order.Lines?.Count ?? 0}");

            var existingOrder = _manager.Order.Orders
                .Include(o => o.Lines)
                .FirstOrDefault(o => o.OrderId == order.OrderId);

            System.Diagnostics.Debug.WriteLine($"🔍 existingOrder bulundu mu? {existingOrder != null}");

            if (existingOrder != null)
            {
                System.Diagnostics.Debug.WriteLine($"🔍 existingOrder.Cancelled={existingOrder.Cancelled}");
                System.Diagnostics.Debug.WriteLine($"🔍 existingOrder.Lines?.Count={existingOrder.Lines?.Count ?? 0}");
            }

            if (existingOrder != null &&
                order.Cancelled &&
                !existingOrder.Cancelled)
            {
                System.Diagnostics.Debug.WriteLine($"✅ IF BLOĞUNA GİRDİ - Stok iade edilecek!");

                foreach (var line in existingOrder.Lines)
                {
                    System.Diagnostics.Debug.WriteLine($"  🔄 İşleniyor: ProductId={line.ProductId}, Size={line.Size}, Qty={line.Quantity}");

                    try
                    {
                        var stock = _manager.ProductStock.GetStockByProductAndSize(
                            line.ProductId,
                            line.Size,
                            true
                        );

                        System.Diagnostics.Debug.WriteLine($"  🔍 Mevcut stok bulundu mu? {stock != null}");

                        if (stock != null)
                        {
                            var oldQty = stock.Quantity;
                            stock.Quantity += line.Quantity;
                            stock.UpdatedAt = DateTime.UtcNow;
                            System.Diagnostics.Debug.WriteLine($"  ✅ Stok güncellendi: {oldQty} -> {stock.Quantity}");
                        }
                        else
                        {
                            var newStock = new ProductStock
                            {
                                ProductId = line.ProductId,
                                Size = line.Size,
                                Quantity = line.Quantity,
                                CreatedAt = DateTime.UtcNow
                            };
                            _manager.ProductStock.CreateStock(newStock);
                            System.Diagnostics.Debug.WriteLine($"  ✅ Yeni stok kaydı oluşturuldu: Qty={line.Quantity}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"  ❌ HATA: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"  ❌ StackTrace: {ex.StackTrace}");
                    }
                }

                _manager.Save();
                System.Diagnostics.Debug.WriteLine($"💾 Stok değişiklikleri kaydedildi!");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"❌ IF BLOĞUNA GİREMEDİ");
            }

            _manager.Order.SaveOrder(order);
            _manager.Save();
            System.Diagnostics.Debug.WriteLine($"💾 Sipariş kaydedildi!");
            System.Diagnostics.Debug.WriteLine($"═══════════════════════════════════════");
        }

        public Task<List<Order>> GetMyOrdersAsync(string userId)
      => _manager.Order.GetByUserAsync(userId);

        public Task<Order?> GetMyOrderAsync(string userId, int id)
            => _manager.Order.GetOneAsync(id, userId);

        public async Task<decimal> GetWeeklySalesAsync()
        {
            var weekAgo = DateTime.UtcNow.AddDays(-7);

            var orders = await _manager.Order.Orders
                .Where(o => o.OrderedAt >= weekAgo && o.Shipped && !o.Cancelled)
                .ToListAsync();

            return orders.Sum(o => o.GrandTotal);
        }

        public async Task<decimal> GetMonthlySalesAsync()
        {
            var monthAgo = DateTime.UtcNow.AddMonths(-1);

            var orders = await _manager.Order.Orders
                .Where(o => o.OrderedAt >= monthAgo && o.Shipped && !o.Cancelled)
                .ToListAsync();

            return orders.Sum(o => o.GrandTotal);
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            var orders = await _manager.Order.Orders
                .Where(o => o.Shipped && !o.Cancelled)
                .ToListAsync();

            return orders.Sum(o => o.GrandTotal);
        }

        public async Task<decimal> GetAverageOrderValueAsync()
        {
            var monthAgo = DateTime.UtcNow.AddMonths(-1);

            var orders = await _manager.Order.Orders
                .Where(o => o.OrderedAt >= monthAgo && o.Shipped && !o.Cancelled)
                .ToListAsync();

            if (!orders.Any()) return 0m;

            return orders.Average(o => o.GrandTotal);
        }

        public async Task<IEnumerable<OrderViewModel>> GetRecentOrdersAsync(int count)
        {
            return await _manager.Order.Orders
                .OrderByDescending(o => o.OrderedAt)
                .Take(count)
                .Select(o => new OrderViewModel
                {
                    OrderId = o.OrderId,
                    OrderNumber = o.OrderId.ToString("000000"),
                    CustomerName = o.Name ?? "Misafir",
                    ItemCount = o.Lines.Count, // Lines navigation yüklüyse çalışır
                    TotalAmount = o.GrandTotal, // ✅ indirimli
                    Status = o.Cancelled ? "İptal" : (o.Shipped ? "Tamamlandı" : "Beklemede"),
                    OrderDate = o.OrderedAt
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<MonthlySalesData>> GetMonthlySalesDataAsync()
        {
            var yearAgo = DateTime.UtcNow.AddMonths(-12);

            var orders = await _manager.Order.Orders
                .Where(o => o.OrderedAt >= yearAgo && o.Shipped && !o.Cancelled)
                .ToListAsync();

            var groupedData = orders
                .GroupBy(o => new { o.OrderedAt.Year, o.OrderedAt.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Amount = g.Sum(o => o.GrandTotal) // ✅ indirimli
                })
                .OrderBy(d => d.Year).ThenBy(d => d.Month)
                .ToList();

            return groupedData.Select(d => new MonthlySalesData
            {
                Month = new DateTime(d.Year, d.Month, 1)
                    .ToString("MMM yyyy", new System.Globalization.CultureInfo("tr-TR")),
                Amount = d.Amount
            });
        }
    }
}