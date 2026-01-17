using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.Contracts;
using Entities.Models;
using StoreApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            var showcaseData = new ShowcaseViewModel
            {
                // 🔥 VİTRİN ÜRÜNLERİ (ShowCase = true olanlar)
                ShowcaseProducts = _manager.PoductService
                    .GetAllProducts(false)
                    .Include(p => p.Stocks)
                    .Where(p => p.ShowCase && p.Stocks.Sum(s => s.Quantity) > 0)
                    .Take(10)
                    .ToList(),

                // ⭐ EN ÇOK FAVORİLENENLER
                MostFavorited = _manager.PoductService
                    .GetAllProducts(false)
                    .Include(p => p.Stocks)
                    .Where(p => p.Stocks.Sum(s => s.Quantity) > 0)
                    .Select(p => new 
                    { 
                        Product = p, 
                        FavCount = _db.UserFavoriteProducts.Count(f => f.ProductId == p.ProductId) 
                    })
                    .OrderByDescending(x => x.FavCount)
                    .Take(10)
                    .Select(x => x.Product)
                    .ToList(),

                // 🆕 EN SON EKLENENLER
                NewArrivals = _manager.PoductService
                    .GetAllProducts(false)
                    .Include(p => p.Stocks)
                    .Where(p => p.Stocks.Sum(s => s.Quantity) > 0)
                    .OrderByDescending(p => p.ProductId)
                    .Take(10)
                    .ToList(),

                // 🔥 ÇOK SATANLAR
                BestSellers = _manager.PoductService
                    .GetAllProducts(false)
                    .Include(p => p.Stocks)
                    .Where(p => p.Stocks.Sum(s => s.Quantity) > 0)
                    .Select(p => new
                    {
                        Product = p,
                        OrderCount = _db.Orders
                            .SelectMany(o => o.Lines)
                            .Count(cl => cl.ProductId == p.ProductId)
                    })
                    .OrderByDescending(x => x.OrderCount)
                    .Take(10)
                    .Select(x => x.Product)
                    .ToList()
            };

            // --- Ratings (avg, count) sözlüğü ---
            var allProductIds = showcaseData.ShowcaseProducts
                .Concat(showcaseData.MostFavorited)
                .Concat(showcaseData.NewArrivals)
                .Concat(showcaseData.BestSellers)
                .Select(p => p.ProductId)
                .Distinct()
                .ToList();

            var ratingData = await _db.Reviews
                .Where(r => r.IsApproved && allProductIds.Contains(r.ProductId))
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

            // Sayfa seçimine göre dön
            return page.Equals("default")
                ? View(showcaseData)
                : View("List", showcaseData);
        }
    }
}