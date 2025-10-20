namespace Repositories
{
    // UserAddressRepository.cs
    using Entities.Models;
    using Microsoft.EntityFrameworkCore;
    using Repositories.Contracts;

    public class UserAddressRepository : IUserAddressRepository
    {
        private readonly RepositoryContext _db;
        public UserAddressRepository(RepositoryContext db) => _db = db;

        public Task<List<UserAddress>> GetAllAsync(string userId)
            => _db.UserAddresses.AsNoTracking()
               .Where(a => a.UserId == userId)
               .OrderByDescending(a => a.IsDefault).ThenByDescending(a => a.CreatedAt)
               .ToListAsync();

        public Task<UserAddress?> GetAsync(int id, string userId, bool trackChanges = false)
            => (trackChanges ? _db.UserAddresses : _db.UserAddresses.AsNoTracking())
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        public async Task CreateAsync(UserAddress address)
            => await _db.UserAddresses.AddAsync(address);

        public Task UpdateAsync(UserAddress address)
        {
            _db.UserAddresses.Update(address);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(UserAddress address)
        {
            _db.UserAddresses.Remove(address);
            return Task.CompletedTask;
        }

        public async Task UnsetDefaultsAsync(string userId)
        {
            var my = await _db.UserAddresses.Where(a => a.UserId == userId && a.IsDefault).ToListAsync();
            foreach (var a in my) a.IsDefault = false;
        }
    }

}