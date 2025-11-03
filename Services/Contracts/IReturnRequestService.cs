using Entities.Models;

namespace Services.Contracts
{
    public interface IReturnRequestService
    {
        IQueryable<ReturnRequest> GetAll();
        ReturnRequest? GetById(int id);
        void Create(ReturnRequest request);
        void Update(ReturnRequest request);
        Task<bool> CanReturnOrder(int orderId);
        IQueryable<ReturnRequest> GetPendingRequests();
    }
}