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

            // ✅ AKILLI ARAMA: Önce kategori adında ara (SADECE SearchTerm varsa)
            if (!string.IsNullOrWhiteSpace(p.SearchTerm))
            {
                var normalizedSearch = p.SearchTerm.Trim().ToLower();

                // Türkçe karakterleri normalize et
                var turkishChars = "çğıöşüÇĞİÖŞÜ";
                var latinChars = "cgiosuCGIOSU";

                for (int i = 0; i < turkishChars.Length; i++)
                {
                    normalizedSearch = normalizedSearch.Replace(turkishChars[i], latinChars[i]);
                }

                // Kategori adında ara
                var categories = _manager.CategoryService.GetAllCategories(false);
                var categoryMatch = categories.FirstOrDefault(c =>
                {
                    var categoryName = c.CategoryName.ToLower();
                    // Türkçe karakterleri normalize et
                    for (int i = 0; i < turkishChars.Length; i++)
                    {
                        categoryName = categoryName.Replace(turkishChars[i], latinChars[i]);
                    }
                    return categoryName.Contains(normalizedSearch);
                });

                if (categoryMatch != null)
                {
                    // Kategori bulundu, o kategorinin ID'sini kullan
                    p.CategoryId = categoryMatch.CategoryId;
                    ViewBag.ActiveCategoryId = categoryMatch.CategoryId;
                    ViewBag.SearchInfo = $"'{categoryMatch.CategoryName}' kategorisindeki ürünler";
                    // SearchTerm'i temizle, kategori filtresi yeterli
                    p.SearchTerm = null;
                }
                else
                {
                    // Kategori bulunamadı, normal ürün araması yap
                    ViewBag.SearchInfo = $"'{p.SearchTerm}' için arama sonuçları";
                }
            }

            // Kullanıcı giriş yaptıysa favoriler
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

            // ✅ Filtrelenmiş sorgu (Include YOK)
            var filtered = _manager.PoductService.GetAllProducts(false)
                .FilteredByCategoryId(p.CategoryId)
                .FilteredBySearchTerm(p.SearchTerm)
                .FilteredByPrice(p.MinPrice, p.MaxPrice, p.IsValidPrice)
                .SortBy(p.SortBy);

            // ✅ Toplam sayıyı al
            var total = await filtered.CountAsync();

            // ✅ Manuel sayfalama + Include en sona
            var products = await filtered
                .Skip((p.PageNumber - 1) * p.PageSize)
                .Take(p.PageSize)
                .Include(p => p.Stocks)
                .ToListAsync();

            // ✅ Ratings hesaplama
            var ids = products.Select(x => x.ProductId).ToList();

            var ratingData = await _db.Reviews
                .Where(r => r.IsApproved && ids.Contains(r.ProductId))
                .GroupBy(r => r.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Count = g.Count(),
                    Avg = g.Average(x => (double)x.Rating)
                })
                .ToListAsync();

            ViewBag.Ratings = ratingData.ToDictionary(
                x => x.ProductId,
                x => (count: x.Count, avg: x.Avg)
            );

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
            var model = _manager.PoductService
                .GetAllProducts(false)
                .Include(p => p.Images)
                .Include(p => p.Stocks)
                .FirstOrDefault(p => p.ProductId == id);

            if (model == null)
                return NotFound();

            // Yorumları include et
            var productWithReviews = _db.Products
                .Include(p => p.Reviews.Where(r => r.IsApproved))
                .ThenInclude(r => r.User)
                .FirstOrDefault(p => p.ProductId == id);

            if (productWithReviews != null)
            {
                model.Reviews = productWithReviews.Reviews;

                // User profilleri için dictionary oluştur
                var userIds = model.Reviews.Select(r => r.UserId).Distinct().ToList();
                var profiles = await _db.UserProfiles
                    .Where(p => userIds.Contains(p.UserId))
                    .ToDictionaryAsync(p => p.UserId, p => p.FullName ?? "Kullanıcı");

                ViewBag.UserProfiles = profiles;
            }

            ViewData["Title"] = model?.ProductName;

            // ✅ Soru-Cevap: sadece cevaplanmışları getir
            var answeredQa = await _manager.ProductQaService.GetAnsweredByProductAsync(id);
            ViewBag.AnsweredQuestions = answeredQa;

            // ✅ Soru soran + admin adları için profilleri çek (opsiyonel ama öneririm)
            var qaUserIds = answeredQa
                .SelectMany(q => new[] { q.UserId, q.Answer!.AdminUserId })
                .Distinct()
                .ToList();

            var qaProfiles = await _db.UserProfiles
                .Where(p => qaUserIds.Contains(p.UserId))
                .ToDictionaryAsync(p => p.UserId, p => p.FullName ?? "Kullanıcı");

            ViewBag.QaUserProfiles = qaProfiles;


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