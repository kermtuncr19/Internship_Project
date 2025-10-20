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
            _manager.Order.SaveOrder(order);
        }

        public Task<List<Order>> GetMyOrdersAsync(string userId)
      => _manager.Order.GetByUserAsync(userId);

        public Task<Order?> GetMyOrderAsync(string userId, int id)
            => _manager.Order.GetOneAsync(id, userId);
    }
}