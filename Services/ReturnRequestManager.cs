using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Contracts;
using Services.Contracts;

namespace Services
{
    public class ReturnRequestManager : IReturnRequestService
    {
        private readonly IRepositoryManager _repository;

        public ReturnRequestManager(IRepositoryManager repository)
        {
            _repository = repository;
        }

        public IQueryable<ReturnRequest> GetAll()
        {
            return _repository.ReturnRequest.GetAll()
                .Include(r => r.Order)
                .ThenInclude(o => o.Lines)
                .ThenInclude(l => l.Product)
                .Include(r => r.Lines)
                .ThenInclude(l => l.CartLine)
                .ThenInclude(c => c.Product);
        }

        public ReturnRequest? GetById(int id)
        {
            return GetAll().FirstOrDefault(r => r.ReturnRequestId == id);
        }

        public void Create(ReturnRequest request)
        {
            _repository.ReturnRequest.Create(request);
            _repository.Save();
        }

        public void Update(ReturnRequest request)
        {
            _repository.ReturnRequest.Update(request);
            _repository.Save();
        }

        public async Task<bool> CanReturnOrder(int orderId)
        {
            var order = await _repository.Order.Orders
    .AsNoTracking()
    .FirstOrDefaultAsync(o => o.OrderId == orderId);


            if (order == null || !order.Delivered || order.Cancelled)
                return false;

            // 15 günlük süre kontrolü
            var returnDeadline = order.DeliveredAt?.AddDays(15);
            return returnDeadline.HasValue && DateTime.UtcNow <= returnDeadline.Value;
        }

        public IQueryable<ReturnRequest> GetPendingRequests()
        {
            return GetAll().Where(r => r.Status == ReturnStatus.Pending);
        }
    }
}