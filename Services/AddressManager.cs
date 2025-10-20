namespace Services
{
    // AddressManager.cs
    using Entities.Models;
    using Repositories.Contracts;
    using Services.Contracts;

    public class AddressManager : IAddressService
    {
        private readonly IRepositoryManager _repo;
        public AddressManager(IRepositoryManager repo) => _repo = repo;

        public Task<List<UserAddress>> GetAllAsync(string userId)
            => _repo.UserAddress.GetAllAsync(userId);

        public Task<UserAddress?> GetAsync(int id, string userId)
            => _repo.UserAddress.GetAsync(id, userId);

        public async Task CreateAsync(string userId, UserAddress dto)
        {
            dto.UserId = userId;
            if (dto.IsDefault) await _repo.UserAddress.UnsetDefaultsAsync(userId);
            dto.CreatedAt = DateTime.UtcNow;
            await _repo.UserAddress.CreateAsync(dto);
            _repo.Save();
        }

        public async Task UpdateAsync(string userId, UserAddress dto)
        {
            var entity = await _repo.UserAddress.GetAsync(dto.Id, userId, trackChanges: true);
            if (entity == null) return;

            if (dto.IsDefault) await _repo.UserAddress.UnsetDefaultsAsync(userId);

            entity.Label = dto.Label;
            entity.City = dto.City;
            entity.District = dto.District;
            entity.Neighborhood = dto.Neighborhood;
            entity.AddressLine = dto.AddressLine;
            entity.PhoneNumber = dto.PhoneNumber;
            entity.IsDefault = dto.IsDefault;

            await _repo.UserAddress.UpdateAsync(entity);
            _repo.Save();
        }

        public async Task DeleteAsync(string userId, int id)
        {
            var entity = await _repo.UserAddress.GetAsync(id, userId, trackChanges: true);
            if (entity == null) return;
            await _repo.UserAddress.DeleteAsync(entity);
            _repo.Save();
        }

        public async Task MakeDefaultAsync(string userId, int id)
        {
            var entity = await _repo.UserAddress.GetAsync(id, userId, trackChanges: true);
            if (entity == null) return;

            await _repo.UserAddress.UnsetDefaultsAsync(userId);
            entity.IsDefault = true;
            await _repo.UserAddress.UpdateAsync(entity);
            _repo.Save();
        }
    }

}