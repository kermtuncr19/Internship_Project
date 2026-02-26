using Entities.Models;

namespace Services.Contracts
{
    public interface ICouponService
    {
        Task<List<Coupon>> GetAllAsync(string? q, bool? active);
        Task<Coupon?> GetByIdAsync(int id, bool trackChanges);
        Task<(bool ok, string? error)> CreateAsync(Coupon model);
        Task<(bool ok, string? error)> UpdateAsync(int id, Coupon model);
        Task<bool> ToggleAsync(int id);
        Task<bool> DeleteAsync(int id);

        // checkout sonrası kullanım sayacı
        Task<bool> IncreaseUsedCountAsync(string code);
        Task<bool> HasUserUsedAsync(string userId, string code);
        Task<(bool ok, string? error)> MarkUsedByUserAsync(string userId, string code, int? orderId);
    }
}