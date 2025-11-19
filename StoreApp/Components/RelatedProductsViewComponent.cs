using Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.Contracts;

namespace StoreApp.Components
{
    public class RelatedProductsViewComponent : ViewComponent
    {
        private readonly IServiceManager _manager;
        private readonly RepositoryContext _db;
        private readonly UserManager<IdentityUser> _um;

        public RelatedProductsViewComponent(IServiceManager manager, RepositoryContext db, UserManager<IdentityUser> um)
        {
            _manager = manager;
            _db = db;
            _um = um;
        }

        public async Task<IViewComponentResult> InvokeAsync(int productId, int? categoryId)
        {
            // Aynı kategorideki diğer ürünleri al (mevcut ürün hariç)
            var relatedProducts = _manager.PoductService.GetAllProducts(false)
                .Include(p => p.Stocks)
                .Where(p => p.ProductId != productId)
                .Where(p => categoryId == null || p.CategoryId == categoryId)
                .OrderByDescending(p => p.ProductId)
                .Take(20)
                .ToList(); // materialize

            // --- Ratings (avg, count) sözlüğü ---
            var ids = relatedProducts.Select(p => p.ProductId).ToList();

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

            // Kullanıcı giriş yaptıysa favori ürünleri
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

            return View(relatedProducts);
        }
    }
}
