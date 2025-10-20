namespace Services
{
    // FavoriteManager.cs
using Entities.Models;
using Repositories.Contracts;
using Services.Contracts;

public class FavoriteManager : IFavoriteService
{
    private readonly IRepositoryManager _repo;
    public FavoriteManager(IRepositoryManager repo) => _repo = repo;

    public async Task ToggleAsync(string userId, int productId)
    {
        var exists = await _repo.Favorite.ExistsAsync(userId, productId);
        if (!exists)
        {
            await _repo.Favorite.AddAsync(new UserFavoriteProduct
            {
                UserId = userId,
                ProductId = productId,
                AddedAt = DateTime.UtcNow
            });
        }
        else
        {
            var fav = new UserFavoriteProduct { UserId = userId, ProductId = productId };
            await _repo.Favorite.RemoveAsync(fav);
        }
        _repo.Save();
    }

    public Task<List<Product>> GetProductsAsync(string userId)
        => _repo.Favorite.GetProductsAsync(userId);
}

}