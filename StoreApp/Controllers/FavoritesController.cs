using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Entities.Models;
using Repositories;
using Services.Contracts;
namespace StoreApp.Controllers
{

    [Authorize]
    public class FavoritesController : Controller
    {
       
        private readonly RepositoryContext _db;
        private readonly UserManager<IdentityUser> _um;

        public FavoritesController(RepositoryContext db, UserManager<IdentityUser> um, IServiceManager manager)
        {
            _db = db; _um = um;
            
        }

        [HttpPost]
        public IActionResult Toggle(int productId, string? returnUrl = "/")
        {
            var userId = _um.GetUserId(User)!;
            var fav = _db.UserFavoriteProducts.FirstOrDefault(f => f.UserId == userId && f.ProductId == productId);
            if (fav == null)
                _db.UserFavoriteProducts.Add(new UserFavoriteProduct { UserId = userId, ProductId = productId, AddedAt = DateTime.UtcNow });
            else
                _db.UserFavoriteProducts.Remove(fav);

            _db.SaveChanges();
            return Redirect(returnUrl ?? "/");
        }

        public IActionResult Index()
        {
            var userId = _um.GetUserId(User)!;
            var products = _db.UserFavoriteProducts
                            .Where(f => f.UserId == userId)
                            .Join(_db.Products, f => f.ProductId, p => p.ProductId, (f, p) => p)
                            .ToList();
            return View(products);
        }
    }

}