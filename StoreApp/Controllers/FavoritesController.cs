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
            _db = db;
            _um = um;
        }

        // ✅ Eski method (form submit için korundu)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Toggle(int productId, string? returnUrl = "/")
        {
            var userId = _um.GetUserId(User)!;
            var fav = _db.UserFavoriteProducts.FirstOrDefault(f => f.UserId == userId && f.ProductId == productId);
            if (fav == null)
                _db.UserFavoriteProducts.Add(new UserFavoriteProduct { UserId = userId, ProductId = productId, AddedAt = DateTime.UtcNow });
            else
                _db.UserFavoriteProducts.Remove(fav);

            _db.SaveChanges();
            // ✅ AJAX isteğiyse sadece OK dön
            if (Request.Headers["X-Requested-With"] == "fetch")
            {
                return Ok();
            }
            return Redirect(returnUrl ?? "/");
        }

        // ✅ YENİ: AJAX için method
        [HttpPost]
        [ValidateAntiForgeryToken]  // CSRF koruması açık
        public IActionResult ToggleAjax([FromBody] FavoriteToggleRequest request)
        {
            try
            {
                var userId = _um.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Not authenticated" });

                var fav = _db.UserFavoriteProducts
                             .FirstOrDefault(f => f.UserId == userId && f.ProductId == request.ProductId);

                bool isFavorite;
                if (fav == null)
                {
                    _db.UserFavoriteProducts.Add(new UserFavoriteProduct
                    {
                        UserId = userId,
                        ProductId = request.ProductId,
                        AddedAt = DateTime.UtcNow
                    });
                    isFavorite = true;
                }
                else
                {
                    _db.UserFavoriteProducts.Remove(fav);
                    isFavorite = false;
                }

                _db.SaveChanges();
                return Json(new { isFavorite });
            }
            catch (Exception ex)
            {
                // Tanı kolaylaştır: 500 yerine açıklamalı 400 dön
                return BadRequest(new { message = ex.Message });
            }
        }

         // ✅ YENİ: Favorilerden KALDIR (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int productId)
        {
            var userId = _um.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Not authenticated" });

            var fav = _db.UserFavoriteProducts
                         .FirstOrDefault(f => f.UserId == userId && f.ProductId == productId);

            if (fav != null)
            {
                _db.UserFavoriteProducts.Remove(fav);
                _db.SaveChanges();
            }

            // Sayfayı yenilemeden DOM'dan sileceğiz
            return Json(new { ok = true, id = productId });
        }

        public IActionResult Index()
        {
            var userId = _um.GetUserId(User)!;
            var favoriteIds = _db.UserFavoriteProducts
                         .Where(f => f.UserId == userId)
                         .Select(f => f.ProductId)
                         .ToList();
            var products = _db.UserFavoriteProducts
                            .Where(f => f.UserId == userId)
                            .Join(_db.Products, f => f.ProductId, p => p.ProductId, (f, p) => p)
                            .ToList();
            ViewBag.FavoriteIds = favoriteIds;
            return View(products);
        }
    }

    // ✅ Request modeli
    public class FavoriteToggleRequest
    {
        public int ProductId { get; set; }
    }
}