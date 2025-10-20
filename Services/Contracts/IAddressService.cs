using Entities.Models;

namespace Services.Contracts
{
    public interface IAddressService
{
    Task<List<UserAddress>> GetAllAsync(string userId);
    Task<UserAddress?> GetAsync(int id, string userId);
    Task CreateAsync(string userId, UserAddress dto);
    Task UpdateAsync(string userId, UserAddress dto);
    Task DeleteAsync(string userId, int id);
    Task MakeDefaultAsync(string userId, int id);
}
}