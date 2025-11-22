using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Contracts;
using System.Linq;

namespace Repositories
{
    public class OrderRepository : RepositoryBase<Order>, IOrderRepository
    {
        public OrderRepository(RepositoryContext context) : base(context) { }

        // Listeleme: satır ve ürünlerle birlikte
        public IQueryable<Order> Orders => _context.Orders
            .Include(o => o.Lines)
                .ThenInclude(cl => cl.Product)
            .OrderBy(o => o.Shipped)
            .ThenByDescending(o => o.OrderId);

        public int NumberOfInProcess => _context.Orders.Count(o => !o.Shipped && !o.Cancelled);

        public void Complete(int id)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == id);
            if (order is null)
                throw new Exception("Sipariş bulunamadı!");

            order.Shipped = true;
            _context.SaveChanges();
        }

        public void Delete(Order order)
        {
            if (order is null) return;
            _context.Orders.Remove(order);
            _context.SaveChanges();
        }

        public Order? GetOneOrder(int id)
        {
            return _context.Orders
                .Include(o => o.Lines)
                    .ThenInclude(cl => cl.Product)
                .FirstOrDefault(o => o.OrderId == id);
        }

        // Repositories/OrderRepository.cs
        public void SaveOrder(Order order)
        {
            // Aynı ürün/beden satırlarını birleştir
            var merged = order.Lines
                .GroupBy(l => new { l.ProductId, l.Size })
                .Select(g => new CartLine
                {
                    ProductId = g.Key.ProductId,
                    Size = g.Key.Size,
                    Quantity = g.Sum(x => x.Quantity),
                    UnitPrice = g.First().UnitPrice
                    // OrderId EF tarafından set edilecek (order'a eklendiği için)
                })
                .ToList();

            order.Lines = merged;
            order.OrderedAt = DateTime.UtcNow;

            // 🔽 Burada değişiklik yaptık:
            if (order.OrderId == 0)
            {
                // Yeni sipariş oluşturuluyor
                _context.Orders.Add(order);
            }
            else
            {
                // Mevcut sipariş (ör. İptal Et veya Güncelle)
                _context.Orders.Update(order);
            }

            _context.SaveChanges();
        }



        // Profil ekranları için:
        public Task<List<Order>> GetByUserAsync(string userId) =>
            _context.Orders.AsNoTracking()
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderedAt)
                .ToListAsync();

        public Task<Order?> GetOneAsync(int id, string userId) =>
            _context.Orders.AsNoTracking()
                .Include(o => o.Lines)
                    .ThenInclude(l => l.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);
    }
}
