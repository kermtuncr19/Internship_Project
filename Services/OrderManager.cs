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

        public async Task<decimal> GetWeeklySalesAsync()
{
    var weekAgo = DateTime.UtcNow.AddDays(-7);
    
    var allOrders = await _manager.Order.Orders.ToListAsync();
    Console.WriteLine($"========== DEBUG GetWeeklySalesAsync ==========");
    Console.WriteLine($"Toplam sipariş: {allOrders.Count}");
    Console.WriteLine($"Shipped=true: {allOrders.Count(o => o.Shipped)}");
    
    // 👇 HER SİPARİŞİN DETAYINI YAZDIR
    foreach (var o in allOrders.Where(o => o.Shipped).Take(5))
    {
        Console.WriteLine($"\nOrder {o.OrderId}:");
        Console.WriteLine($"  Lines Count: {o.Lines.Count}");
        
        foreach (var line in o.Lines)
        {
            Console.WriteLine($"    Product {line.ProductId}: UnitPrice=₺{line.UnitPrice}, Quantity={line.Quantity}, Total=₺{line.UnitPrice * line.Quantity}");
        }
        
        var orderTotal = o.Lines.Sum(l => l.UnitPrice * l.Quantity);
        Console.WriteLine($"  Order Total: ₺{orderTotal}");
    }
    // 👆
    
    var orders = await _manager.Order.Orders
        .Where(o => o.OrderedAt >= weekAgo && o.Shipped && !o.Cancelled)
        .Include(o => o.Lines)
        .ToListAsync();

    Console.WriteLine($"\nBu hafta tamamlanan sipariş: {orders.Count}");
    var weeklyTotal = orders.Sum(o => o.Lines.Sum(l => l.UnitPrice * l.Quantity));
    Console.WriteLine($"Haftalık toplam: ₺{weeklyTotal}");
    Console.WriteLine($"==========================================");
    
    return weeklyTotal;
}

        public async Task<decimal> GetMonthlySalesAsync()
        {
            var monthAgo = DateTime.UtcNow.AddMonths(-1);
            var orders = await _manager.Order.Orders
                .Where(o => o.OrderedAt >= monthAgo && o.Shipped && !o.Cancelled)
                .Include(o => o.Lines)
                .ToListAsync();

            return orders.Sum(o => o.Lines.Sum(l => l.UnitPrice * l.Quantity));
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            var orders = await _manager.Order.Orders
                .Where(o => o.Shipped && !o.Cancelled)
                .Include(o => o.Lines)
                .ToListAsync();

            return orders.Sum(o => o.Lines.Sum(l => l.UnitPrice * l.Quantity));
        }

        public async Task<decimal> GetAverageOrderValueAsync()
        {
            var monthAgo = DateTime.UtcNow.AddMonths(-1);
            var orders = await _manager.Order.Orders
                .Where(o => o.OrderedAt >= monthAgo && o.Shipped && !o.Cancelled)
                .Include(o => o.Lines)
                .ToListAsync();

            if (!orders.Any()) return 0;

            var orderTotals = orders.Select(o => o.Lines.Sum(l => l.UnitPrice * l.Quantity));
            return orderTotals.Average();
        }

        public async Task<IEnumerable<OrderViewModel>> GetRecentOrdersAsync(int count)
        {
            return await _manager.Order.Orders
                .OrderByDescending(o => o.OrderedAt)
                .Take(count)
                .Include(o => o.Lines)
                .Select(o => new OrderViewModel
                {
                    OrderId = o.OrderId,
                    OrderNumber = o.OrderId.ToString("000000"),
                    CustomerName = o.Name ?? "Misafir",
                    ItemCount = o.Lines.Count,
                    TotalAmount = o.Lines.Sum(l => l.UnitPrice * l.Quantity),
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
                .Include(o => o.Lines)
                .ToListAsync();

            var groupedData = orders
                .GroupBy(o => new { o.OrderedAt.Year, o.OrderedAt.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Amount = g.Sum(o => o.Lines.Sum(l => l.UnitPrice * l.Quantity))
                })
                .OrderBy(d => d.Year).ThenBy(d => d.Month)
                .ToList();

            return groupedData.Select(d => new MonthlySalesData
            {
                Month = new DateTime(d.Year, d.Month, 1).ToString("MMM yyyy", new System.Globalization.CultureInfo("tr-TR")),
                Amount = d.Amount
            });
        }
    }
}