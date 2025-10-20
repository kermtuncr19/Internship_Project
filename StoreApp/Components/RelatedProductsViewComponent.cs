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
                .Where(p => p.ProductId != productId)
                .Where(p => categoryId == null || p.CategoryId == categoryId)
                .Take(20) // İlk 20 ürünü al
                .ToList();

            // Kullanıcı giriş yaptıysa favori ürünlerini al
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