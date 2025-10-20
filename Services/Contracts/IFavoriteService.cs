using Entities.Models;

namespace Services.Contracts
{
    public interface IFavoriteService
{
    Task ToggleAsync(string userId, int productId);
    Task<List<Product>> GetProductsAsync(string userId);
}
}