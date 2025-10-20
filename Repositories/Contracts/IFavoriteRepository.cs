using Entities.Models;

namespace Repositories.Contracts
{
    public interface IFavoriteRepository
{
    Task<bool> ExistsAsync(string userId, int productId);
    Task AddAsync(UserFavoriteProduct fav);
    Task RemoveAsync(UserFavoriteProduct fav);
    Task<List<Product>> GetProductsAsync(string userId);
}
}