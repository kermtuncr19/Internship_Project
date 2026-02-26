using Entities.Models;

namespace Repositories.Contracts
{
    public interface ICouponRepository
    {
        IQueryable<Coupon> GetAll(bool trackChanges);
        Task<Coupon?> GetByIdAsync(int id, bool trackChanges);
        Task<Coupon?> GetByCodeAsync(string code, bool trackChanges);
        Task<bool> CodeExistsAsync(string code, int? excludeId = null);
        void Create(Coupon coupon);
        void Delete(Coupon coupon);
        Task SaveAsync();
    }
}