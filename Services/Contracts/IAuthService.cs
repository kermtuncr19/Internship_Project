using Entities.Dto;
using Entities.Models;
using Microsoft.AspNetCore.Identity;

namespace Services.Contracts
{
    public interface IAuthService
    {
        IEnumerable<IdentityRole> Roles { get; }
        IEnumerable<IdentityUser> GetAllUsers();
        Task<IdentityResult> CreateUser(UserDtoForCreation userDto);
        Task<IdentityUser> GetOneUser(string userName);
        Task<UserDtoForUpdate> GetOneUserForUpdate(string userName);
        Task Update(UserDtoForUpdate userDto);
        Task<IdentityResult> ResetPassword(ResetPasswordDto model);
        Task<IdentityResult> DeleteOneUser(string userName);

        Task<int> GetTotalUserCountAsync();
        Task<IEnumerable<ActivityViewModel>> GetRecentActivitiesAsync(int count);
        
    }
}