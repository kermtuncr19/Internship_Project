using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories;
using StoreApp.Models;
using Services.Contracts;
using StoreApp.Infrastructure.Extensions;


namespace StoreApp.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly RepositoryContext _ctx;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IStorageService _storage;

        public ProfileController(RepositoryContext ctx, UserManager<IdentityUser> userManager, IStorageService storage)
        {
            _ctx = ctx;
            _userManager = userManager;
            _storage = storage;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var user = await _userManager.FindByIdAsync(userId);

            var profile = await _ctx.UserProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId);

            var lastOrders = await _ctx.Orders
                .AsNoTracking()
                .Include(o => o.Lines)
                    .ThenInclude(l => l.Product)
                        .ThenInclude(p => p.Images)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderedAt)
                .Take(5)
                .ToListAsync();

            var addressCount = await _ctx.UserAddresses
                .AsNoTracking()
                .CountAsync(a => a.UserId == userId);

            var favoriteCount = await _ctx.UserFavoriteProducts
                .AsNoTracking()
                .CountAsync(f => f.UserId == userId);

            var viewModel = new ProfileDashboardViewModel
            {
                Profile = profile,
                LastOrders = lastOrders,
                AddressCount = addressCount,
                FavoriteCount = favoriteCount,
                Email = user?.Email
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = _userManager.GetUserId(User)!;

            var profile = await _ctx.UserProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                profile = new UserProfile { UserId = userId };
            }

            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserProfile formModel, IFormFile? avatar)
        {
            var userId = _userManager.GetUserId(User)!;

            var entity = await _ctx.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            var isNew = entity == null;

            if (entity == null)
                entity = new UserProfile { UserId = userId };

            entity.FullName = formModel.FullName;
            entity.PhoneNumber = formModel.PhoneNumber;
            entity.BirthDate = formModel.BirthDate;

            // ✅ Avatarı bucket'a yükle (wwwroot'a yazma yok)
            if (avatar != null && avatar.Length > 0)
            {

                if (!avatar.IsValidImage(2, out var avatarError))
                {
                    ModelState.AddModelError("avatar", avatarError);
                    return View(formModel);
                }

                // images/avatars altına atsın
                var (_, publicUrl) = await _storage.UploadAsync(avatar, "images/avatars");
                entity.AvatarUrl = publicUrl;
            }

            if (isNew)
                _ctx.UserProfiles.Add(entity);
            else
                _ctx.UserProfiles.Update(entity);

            await _ctx.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
