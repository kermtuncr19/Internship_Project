namespace Repositories
{
    // UserProfileRepository.cs
    using Entities.Models;
    using Microsoft.EntityFrameworkCore;
    using Repositories.Contracts;

    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly RepositoryContext _db;
        public UserProfileRepository(RepositoryContext db) => _db = db;

        public async Task<UserProfile?> GetByUserIdAsync(string userId, bool trackChanges = false)
            => trackChanges
                ? await _db.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId)
                : await _db.UserProfiles.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId);

        public async Task UpsertAsync(UserProfile profile)
        {
            var existing = await _db.UserProfiles.FirstOrDefaultAsync(x => x.UserId == profile.UserId);
            if (existing == null)
                await _db.UserProfiles.AddAsync(profile);
            else
            {
                existing.FullName = profile.FullName;
                existing.PhoneNumber = profile.PhoneNumber;
                existing.AvatarUrl = profile.AvatarUrl;
                existing.BirthDate = profile.BirthDate;
            }
        }
    }

}