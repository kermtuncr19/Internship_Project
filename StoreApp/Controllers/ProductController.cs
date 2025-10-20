using Entities.Models;
using Entities.RequestParameters;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Repositories.Contracts;
using Services.Contracts;
using StoreApp.Models;
using Repositories.Extensions;


namespace StoreApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly IServiceManager _manager;
        private readonly RepositoryContext _db;
        private readonly UserManager<IdentityUser> _um;

        public ProductController(IServiceManager manager, RepositoryContext db, UserManager<IdentityUser> um)
        {
            _manager = manager;
            _db = db;
            _um = um;
        }

        public async Task<IActionResult> Index(ProductRequestParameters p)
        {
            ViewData["Title"] = "Ürünler";
            if (!p.IsValidPrice)
            {
                TempData["PriceError"] = "Maksimum fiyat, minimum fiyattan küçük olamaz.";
                p.MinPrice = 0;
                p.MaxPrice = int.MaxValue;
            }

            ViewBag.Categories = _manager.CategoryService.GetAllCategories(false);
            ViewBag.ActiveCategoryId = p.CategoryId;

            // Kullanıcı giriş yaptıysa favori ürünlerini al
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _um.GetUserId(User)!;
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

            // 1) Filtrelenmiş temel sorgu (henüz pagination yok)
            var filtered = _manager.PoductService.GetAllProducts(false)
            .FilteredByCategoryId(p.CategoryId)
            .FilteredBySearchTerm(p.SearchTerm)
            .FilteredByPrice(p.MinPrice, p.MaxPrice, p.IsValidPrice)
             .OrderBy(pr => pr.ProductId);

            // 2) Sayfalık veri
            var products = filtered.ToPaginate(p.PageNumber, p.PageSize);

            // 3) Toplam eleman sayısı (aynen aynı filtrelerle)
            var total = filtered.Count();

            var pagination = new Pagination
            {
                CurrentPage = p.PageNumber < 1 ? 1 : p.PageNumber,
                ItemsPerPage = p.PageSize < 1 ? 10 : p.PageSize,
                TotalItems = total
            };

            return View(new ProductListViewModel
            {
                Products = products,
                Pagination = pagination
            });
        }

        public async Task<IActionResult> Get([FromRoute(Name = "id")] int id)
        {
            var model = _manager.PoductService.GetOneProduct(id, false);

            if (model == null)
                return NotFound();

            ViewData["Title"] = model?.ProductName;

            // Kullanıcı giriş yaptıysa bu ürünün favori durumunu kontrol et
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _um.GetUserId(User)!;
                ViewBag.IsFavorite = await _db.UserFavoriteProducts
                    .AnyAsync(f => f.UserId == userId && f.ProductId == id);
            }
            else
            {
                ViewBag.IsFavorite = false;
            }

            return View(model);
        }
    }
}