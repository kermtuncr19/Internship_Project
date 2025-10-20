using Entities.Models;

namespace Repositories.Contracts
{
    public interface IUserAddressRepository
{
    Task<List<UserAddress>> GetAllAsync(string userId);
    Task<UserAddress?> GetAsync(int id, string userId, bool trackChanges = false);
    Task CreateAsync(UserAddress address);
    Task UpdateAsync(UserAddress address);
    Task DeleteAsync(UserAddress address);
    Task UnsetDefaultsAsync(string userId);
}
}