using AutoMapper;
using Entities.Dto;
using Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Services.Contracts;

namespace Services
{
    public class AuthManager : IAuthService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IOrderService _orderService;

        public AuthManager(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, IMapper mapper, IOrderService orderService)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _mapper = mapper;
            _orderService = orderService;
        }

        public IEnumerable<IdentityRole> Roles =>
            _roleManager.Roles;

        public async Task<IdentityResult> CreateUser(UserDtoForCreation userDto)
        {
            var user = _mapper.Map<IdentityUser>(userDto);
            var result = await _userManager.CreateAsync(user, userDto.Password!);

            if (!result.Succeeded)
            {
                throw new Exception("Kullanıcı Oluşturulamadı!");
            }
            if (userDto.Roles.Count > 0)
            {
                var roleResult = await _userManager.AddToRolesAsync(user, userDto.Roles);
                if (!roleResult.Succeeded)
                    throw new Exception("Rollerle İlgili Bir Sorun Var!");
            }
            return result;
        }

        public async Task<IdentityResult> DeleteOneUser(string userName)
        {
            var user = await GetOneUser(userName);
            var result = await _userManager.DeleteAsync(user);
            return result;

        }

        public IEnumerable<IdentityUser> GetAllUsers()
        {
            return _userManager.Users.ToList();
        }

        public async Task<IdentityUser> GetOneUser(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user is not null)
                return user;
            throw new Exception("Kullanıcı Bulunamadı!");
        }

        public async Task<UserDtoForUpdate> GetOneUserForUpdate(string userName)
        {
            var user = await GetOneUser(userName);

            var userDto = _mapper.Map<UserDtoForUpdate>(user);
            userDto.Roles = new HashSet<string>(Roles.Select(r => r.Name!).ToList());
            userDto.UserRoles = new HashSet<string>(await _userManager.GetRolesAsync(user));
            return userDto;


        }

        public async Task<IdentityResult> ResetPassword(ResetPasswordDto model)
        {
            var user = await GetOneUser(model.UserName);

            await _userManager.RemovePasswordAsync(user);
            var result = await _userManager.AddPasswordAsync(user, model.Password);
            return result;

        }

        public async Task Update(UserDtoForUpdate userDto)
        {
            var user = await GetOneUser(userDto.UserName!);

            user.PhoneNumber = userDto.PhoneNumber;
            user.Email = userDto.Email;

            // Kullanıcı bilgilerini güncelle
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                throw new Exception("Kullanıcı güncellenirken bir hata oluştu!");
            }

            // Rolleri güncelle (UserRoles kullan, Roles değil!)
            if (userDto.UserRoles != null)
            {
                // Mevcut rolleri al
                var currentRoles = await _userManager.GetRolesAsync(user);

                // Mevcut rolleri kaldır
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

                if (!removeResult.Succeeded)
                {
                    throw new Exception("Roller kaldırılırken bir hata oluştu!");
                }

                // Yeni rolleri ekle (formdan gelen UserRoles'u kullan)
                if (userDto.UserRoles.Count > 0)
                {
                    var addResult = await _userManager.AddToRolesAsync(user, userDto.UserRoles);

                    if (!addResult.Succeeded)
                    {
                        throw new Exception("Roller eklenirken bir hata oluştu!");
                    }
                }
            }
        }

        public async Task<int> GetTotalUserCountAsync()
        {
            return await _userManager.Users.CountAsync();
        }

        public async Task<IEnumerable<ActivityViewModel>> GetRecentActivitiesAsync(int count)
        {
            var activities = new List<ActivityViewModel>();

            // Son siparişlerden aktivite oluştur
            var recentOrders = await _orderService.GetRecentOrdersAsync(5);

            activities.AddRange(recentOrders.Select(o => new ActivityViewModel
            {
                Type = "Order",
                Title = "Yeni Sipariş",
                Description = $"{o.CustomerName} tarafından #{o.OrderNumber} sipariş verildi - ₺{o.TotalAmount:N2}",
                CreatedAt = o.OrderDate
            }));

            return activities
                .OrderByDescending(a => a.CreatedAt)
                .Take(count);
        }

        public async Task<IList<string>> GetUserRolesAsync(string userName)
        {
            var user = await GetOneUser(userName);
            return await _userManager.GetRolesAsync(user);
        }
    }
}