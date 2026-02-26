using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Contracts;

namespace Repositories
{
    public class CouponRepository : ICouponRepository
    {
        private readonly RepositoryContext _context;

        public CouponRepository(RepositoryContext context)
        {
            _context = context;
        }

        public IQueryable<Coupon> GetAll(bool trackChanges) =>
            trackChanges
                ? _context.Coupons
                : _context.Coupons.AsNoTracking();

        public Task<Coupon?> GetByIdAsync(int id, bool trackChanges) =>
            GetAll(trackChanges).FirstOrDefaultAsync(x => x.CouponId == id);

        public Task<Coupon?> GetByCodeAsync(string code, bool trackChanges) =>
            GetAll(trackChanges).FirstOrDefaultAsync(x => x.Code == code);

        public Task<bool> CodeExistsAsync(string code, int? excludeId = null)
        {
            var q = _context.Coupons.AsQueryable().Where(x => x.Code == code);
            if (excludeId.HasValue) q = q.Where(x => x.CouponId != excludeId.Value);
            return q.AnyAsync();
        }

        public void Create(Coupon coupon) => _context.Coupons.Add(coupon);

        public void Delete(Coupon coupon) => _context.Coupons.Remove(coupon);

        public Task SaveAsync() => _context.SaveChangesAsync();
    }
}