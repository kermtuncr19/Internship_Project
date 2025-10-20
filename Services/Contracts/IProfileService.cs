using Entities.Models;

namespace Services.Contracts
{
    public interface IProfileService
{
    Task<UserProfile> GetOrCreateAsync(string userId);
    Task UpdateAsync(string userId, UserProfile model);
}
}