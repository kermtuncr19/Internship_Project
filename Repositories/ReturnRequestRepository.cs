using Entities.Models;
using Repositories.Contracts;

namespace Repositories
{
    public class ReturnRequestRepository : RepositoryBase<ReturnRequest>, IReturnRequestRepository
    {
        public ReturnRequestRepository(RepositoryContext context) : base(context)
        {
        }

        public IQueryable<ReturnRequest> GetAll()
        {
            return _context.ReturnRequests;
        }
    }
}