using Entities.Models;
using Entities.RequestParameters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Repositories.Contracts;
using Services.Contracts;


namespace StoreApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly IServiceManager _manager;//dependency injection

        public ProductController(IServiceManager manager)
        {
            _manager = manager;
        }

        public IActionResult Index(ProductRequestParameters p)
        {
            if (!p.IsValidPrice)
            {
                TempData["PriceError"] = "Maksimum fiyat, minimum fiyattan küçük olamaz.";
                // İki seçenekten birini yap:
                // 1) Fiyat filtresini yok say (en güvenlisi):
                p.MinPrice = 0;
                p.MaxPrice = int.MaxValue;

                // 2) Veya otomatik düzelt (swap)
                // (var tmp = p.MinPrice; p.MinPrice = p.MaxPrice; p.MaxPrice = tmp;)
            }

            ViewBag.Categories = _manager.CategoryService.GetAllCategories(false); // IEnumerable<Category> dönen bir metod
            ViewBag.ActiveCategoryId = p.CategoryId;
            var model = _manager.PoductService.GetAllProductsWithDetails(p);
            return View(model);
        }

        public IActionResult Get([FromRoute(Name = "id")] int id)
        {
            //  Product product = _context.Products.First(p => p.Id.Equals(id));
            var model = _manager.PoductService.GetOneProduct(id, false);
            return View(model);
        }
    }

}