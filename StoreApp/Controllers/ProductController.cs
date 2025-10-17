using Entities.Models;
using Entities.RequestParameters;
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
        private readonly IServiceManager _manager;//dependency injection

        public ProductController(IServiceManager manager)
        {
            _manager = manager;
        }

        public IActionResult Index(ProductRequestParameters p)
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
                ItemsPerPage = p.PageSize < 1 ? 6 : p.PageSize,
                TotalItems = total
            };

            return View(new ProductListViewModel
            {
                Products = products,
                Pagination = pagination
            });
        }

        public IActionResult Get([FromRoute(Name = "id")] int id)
        {
            //  Product product = _context.Products.First(p => p.Id.Equals(id));
            var model = _manager.PoductService.GetOneProduct(id, false);
            ViewData["Title"] = model?.ProductName;
            return View(model);
        }
    }

}