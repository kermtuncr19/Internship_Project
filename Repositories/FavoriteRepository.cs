namespace Repositories
{
    // FavoriteRepository.cs
    using Entities.Models;
    using Microsoft.EntityFrameworkCore;
    using Repositories.Contracts;

    public class FavoriteRepository : IFavoriteRepository
    {
        private readonly RepositoryContext _db;
        public FavoriteRepository(RepositoryContext db) => _db = db;

        public Task<bool> ExistsAsync(string userId, int productId)
            => _db.UserFavoriteProducts.AnyAsync(f => f.UserId == userId && f.ProductId == productId);

        public async Task AddAsync(UserFavoriteProduct fav)
            => await _db.UserFavoriteProducts.AddAsync(fav);

        public Task RemoveAsync(UserFavoriteProduct fav)
        {
            _db.UserFavoriteProducts.Remove(fav);
            return Task.CompletedTask;
        }

        public async Task<List<Product>> GetProductsAsync(string userId)
        {
            var query =
                from f in _db.UserFavoriteProducts.AsNoTracking()
                join p in _db.Products.AsNoTracking() on f.ProductId equals p.ProductId
                where f.UserId == userId
                select p;

            return await query.ToListAsync();
        }
    }

}