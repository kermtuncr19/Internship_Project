using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories;
using StoreApp.Models; // ⬅️ ViewModel için

namespace StoreApp.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly RepositoryContext _ctx;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public ProfileController(RepositoryContext ctx, UserManager<IdentityUser> userManager, IWebHostEnvironment env)
        {
            _ctx = ctx;
            _userManager = userManager;
            _env = env;
        }

        // GET: /Profile veya /Profile/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;

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

            // 👇 Burayı AKTİF et
            var addressCount = await _ctx.UserAddresses
                .AsNoTracking()
                .CountAsync(a => a.UserId == userId);

            // Favorites tablon yoksa 0 bırak; varsa aynı şekilde say:
            var favoriteCount = await _ctx.UserFavoriteProducts
                .AsNoTracking()
                .CountAsync(f => f.UserId == userId);

            var viewModel = new ProfileDashboardViewModel
            {
                Profile = profile,
                LastOrders = lastOrders,
                AddressCount = addressCount,
                FavoriteCount = favoriteCount
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

            if (avatar != null && avatar.Length > 0)
            {
                var avatarsRoot = Path.Combine(_env.WebRootPath, "images", "avatars");
                Directory.CreateDirectory(avatarsRoot);

                var ext = Path.GetExtension(avatar.FileName);
                var fileName = $"{userId}_{Guid.NewGuid():N}{ext}";
                var fullPath = Path.Combine(avatarsRoot, fileName);

                using (var stream = System.IO.File.Create(fullPath))
                {
                    await avatar.CopyToAsync(stream);
                }

                entity.AvatarUrl = $"/images/avatars/{fileName}";
            }

            if (isNew)
                _ctx.UserProfiles.Add(entity);
            else
                _ctx.UserProfiles.Update(entity);

            await _ctx.SaveChangesAsync();

            return RedirectToAction(nameof(Index)); // ⬅️ Index'e yönlendir
        }
    }
}