using Entities.Models;

namespace Repositories.Contracts
{
    public interface IUserProfileRepository
{
    Task<UserProfile?> GetByUserIdAsync(string userId, bool trackChanges = false);
    Task UpsertAsync(UserProfile profile);
}
}