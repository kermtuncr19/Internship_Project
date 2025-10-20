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
            var products = _manager.PoductService.GetShowcaseProducts(false);

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

            return page.Equals("default")
                ? View(products)
                : View("List", products);
        }
    }
}