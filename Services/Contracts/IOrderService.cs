using Entities.Models;

namespace Services.Contracts
{
    public interface IOrderService
    {
        IQueryable<Order> Orders { get; }
        Order? GetOneOrder(int id);
        void Complete(int id);
        void Delete(int id);
        void SaveOrder(Order order);

        int NumberOfInProcess { get; }

        Task<List<Order>> GetMyOrdersAsync(string userId);
        Task<Order?> GetMyOrderAsync(string userId, int id);
    }
}