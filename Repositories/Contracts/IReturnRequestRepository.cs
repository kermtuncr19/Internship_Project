using Entities.Models;

namespace Repositories.Contracts
{
    public interface IReturnRequestRepository : IRepositoryBase<ReturnRequest>
    {
        IQueryable<ReturnRequest> GetAll();
    }
}