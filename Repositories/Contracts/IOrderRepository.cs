using Entities.Models;

namespace Repositories.Contracts
{
    public interface IOrderRepository
    {
        IQueryable<Order> Orders { get; }
        Order? GetOneOrder(int id);
        void Complete(int id);
        void SaveOrder(Order order);
        void Delete(Order order);
        int NumberOfInProcess { get; }

        Task<List<Order>> GetByUserAsync(string userId);
        Task<Order?> GetOneAsync(int id, string userId);

    }
}