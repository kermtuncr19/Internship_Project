using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Repositories.Contracts;
using Services.Contracts;

namespace Services
{
    public class CouponService : ICouponService
    {
        private readonly ICouponRepository _repo;
        private readonly RepositoryContext _context;

        public CouponService(ICouponRepository repo, RepositoryContext context)
        {
            _repo = repo;
            _context = context;
        }

        public async Task<List<Coupon>> GetAllAsync(string? q, bool? active)
        {
            var coupons = _repo.GetAll(trackChanges: false);

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim().ToUpperInvariant();
                coupons = coupons.Where(c => c.Code.Contains(q));
            }

            if (active.HasValue)
                coupons = coupons.Where(c => c.IsActive == active.Value);

            return await Task.FromResult(
                coupons.OrderByDescending(c => c.CouponId).ToList()
            );
        }

        public Task<Coupon?> GetByIdAsync(int id, bool trackChanges) =>
            _repo.GetByIdAsync(id, trackChanges);

        public async Task<(bool ok, string? error)> CreateAsync(Coupon model)
        {
            model.Code = (model.Code ?? "").Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(model.Code))
                return (false, "Kupon kodu gerekli.");

            if (model.Percent < 0 || model.Percent > 100)
                return (false, "Yüzde 0-100 aralığında olmalı.");

            if (model.StartsAtUtc.HasValue && model.EndsAtUtc.HasValue && model.EndsAtUtc < model.StartsAtUtc)
                return (false, "Bitiş, başlangıçtan küçük olamaz.");

            if (await _repo.CodeExistsAsync(model.Code))
                return (false, "Bu kupon kodu zaten var.");

            model.UsedCount = 0;

            _repo.Create(model);
            await _repo.SaveAsync();

            return (true, null);
        }

        public async Task<(bool ok, string? error)> UpdateAsync(int id, Coupon model)
        {
            var entity = await _repo.GetByIdAsync(id, trackChanges: true);
            if (entity == null) return (false, "Kupon bulunamadı.");

            model.Code = (model.Code ?? "").Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(model.Code))
                return (false, "Kupon kodu gerekli.");

            if (model.Percent < 0 || model.Percent > 100)
                return (false, "Yüzde 0-100 aralığında olmalı.");

            if (model.StartsAtUtc.HasValue && model.EndsAtUtc.HasValue && model.EndsAtUtc < model.StartsAtUtc)
                return (false, "Bitiş, başlangıçtan küçük olamaz.");

            if (await _repo.CodeExistsAsync(model.Code, excludeId: id))
                return (false, "Bu kupon kodu zaten var.");

            entity.Code = model.Code;
            entity.Percent = model.Percent;
            entity.IsActive = model.IsActive;
            entity.MinCartTotal = model.MinCartTotal;
            entity.StartsAtUtc = model.StartsAtUtc;
            entity.EndsAtUtc = model.EndsAtUtc;
            entity.UsageLimit = model.UsageLimit;

            await _repo.SaveAsync();
            return (true, null);
        }

        public async Task<bool> ToggleAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id, trackChanges: true);
            if (entity == null) return false;

            entity.IsActive = !entity.IsActive;
            await _repo.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id, trackChanges: true);
            if (entity == null) return false;

            _repo.Delete(entity);
            await _repo.SaveAsync();
            return true;
        }

        public async Task<bool> IncreaseUsedCountAsync(string code)
        {
            code = (code ?? "").Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(code)) return false;

            var c = await _repo.GetByCodeAsync(code, trackChanges: true);
            if (c == null) return false;

            c.UsedCount += 1;

            // ✅ Limit dolduysa otomatik pasife çek
            if (c.UsageLimit.HasValue && c.UsedCount >= c.UsageLimit.Value)
                c.IsActive = false;

            await _repo.SaveAsync();
            return true;
        }

        public async Task<bool> HasUserUsedAsync(string userId, string code)
        {
            code = (code ?? "").Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code))
                return false;

            var coupon = await _repo.GetByCodeAsync(code, trackChanges: false);
            if (coupon == null) return false;

            return await _context.CouponUsages
                .AsNoTracking()
                .AnyAsync(x => x.UserId == userId && x.CouponId == coupon.CouponId);
        }

        public async Task<(bool ok, string? error)> MarkUsedByUserAsync(string userId, string code, int? orderId)
        {
            code = (code ?? "").Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code))
                return (false, "Kupon kodu geçersiz.");

            await using var tx = await _context.Database.BeginTransactionAsync();

            var c = await _repo.GetByCodeAsync(code, trackChanges: true);
            if (c == null) return (false, "Kupon bulunamadı.");
            if (!c.IsActive) return (false, "Kupon aktif değil.");

            var now = DateTime.UtcNow;
            if (c.StartsAtUtc.HasValue && now < c.StartsAtUtc.Value) return (false, "Kupon henüz başlamadı.");
            if (c.EndsAtUtc.HasValue && now > c.EndsAtUtc.Value) return (false, "Kuponun süresi doldu.");

            // ✅ daha önce kullandı mı?
            var usedBefore = await _context.CouponUsages
                .AnyAsync(x => x.UserId == userId && x.CouponId == c.CouponId);

            if (usedBefore)
                return (false, "Bu kuponu daha önce kullandınız.");

            _context.CouponUsages.Add(new CouponUsage
            {
                UserId = userId,
                CouponId = c.CouponId,
                OrderId = orderId,
                UsedAtUtc = DateTime.UtcNow
            });

            // ✅ global usedCount + limit
            c.UsedCount += 1;
            if (c.UsageLimit.HasValue && c.UsedCount >= c.UsageLimit.Value)
                c.IsActive = false;

            try
            {
                await _context.SaveChangesAsync(); // unique index burada da korur
                await tx.CommitAsync();
                return (true, null);
            }
            catch (DbUpdateException)
            {
                await tx.RollbackAsync();
                return (false, "Bu kuponu daha önce kullandınız.");
            }
        }
    }
}