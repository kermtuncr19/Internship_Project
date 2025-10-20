namespace Services
{
    // ProfileManager.cs
    using Entities.Models;
    using Repositories.Contracts;
    using Services.Contracts;

    public class ProfileManager : IProfileService
    {
        private readonly IRepositoryManager _repo;
        public ProfileManager(IRepositoryManager repo) => _repo = repo;

        public async Task<UserProfile> GetOrCreateAsync(string userId)
        {
            var p = await _repo.UserProfile.GetByUserIdAsync(userId);
            if (p != null) return p;
            p = new UserProfile { UserId = userId };
            await _repo.UserProfile.UpsertAsync(p);
            _repo.Save();
            return p;
        }

        public async Task UpdateAsync(string userId, UserProfile model)
        {
            model.UserId = userId;
            await _repo.UserProfile.UpsertAsync(model);
            _repo.Save();
        }
       
    }

}