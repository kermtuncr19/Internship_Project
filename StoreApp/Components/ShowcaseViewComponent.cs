using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.Contracts;

namespace StoreApp.Components
{
    public class ShowcaseViewComponent : ViewComponent
    {
        private readonly IServiceManager _manager;
        private readonly RepositoryContext _db;
        private readonly UserManager<IdentityUser> _um;

        public ShowcaseViewComponent(IServiceManager manager, RepositoryContext db, UserManager<IdentityUser> um)
        {
            _manager = manager;
            _db = db;
            _um = um;
        }

        public async Task<IViewComponentResult> InvokeAsync(string page = "default")
        {
            var products = _manager.PoductService.GetShowcaseProducts(false).ToList();

            // --- Ratings (avg, count) sözlüğü ---
            var ids = products.Select(p => p.ProductId).ToList();

            var ratingData = await _db.Reviews
                .Where(r => r.IsApproved && ids.Contains(r.ProductId))
                .GroupBy(r => r.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Count = g.Count(),
                    Avg = g.Average(x => x.Rating)
                })
                .ToListAsync();

            var ratingsDict = ratingData.ToDictionary(
                x => x.ProductId,
                x => (count: x.Count, avg: x.Avg)
            );

            // Hem ViewBag hem ViewData ile ver
            ViewBag.Ratings = ratingsDict;
            ViewData["Ratings"] = ratingsDict;

            // Kullanıcı giriş yaptıysa favoriler
            if (HttpContext.User.Identity?.IsAuthenticated == true)
            {
                var userId = _um.GetUserId(HttpContext.User)!;
                var favoriteIds = await _db.UserFavoriteProducts
                    .Where(f => f.UserId == userId)
                    .Select(f => f.ProductId)
                    .ToListAsync();

                ViewBag.FavoriteIds = favoriteIds;
            }
            else
            {
                ViewBag.FavoriteIds = new List<int>();
            }

            return page.Equals("default")
                ? View(products)
                : View("List", products);
        }
    }
}
