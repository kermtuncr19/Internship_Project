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
             ViewBag.Categories = _manager.CategoryService.GetAllCategories(false); // IEnumerable<Category> dönen bir metod
            ViewBag.ActiveCategoryId = p.CategoryId;
            var model = _manager.PoductService.GetAllProductsWithDetails(p);
            return View(model);
        }

        public IActionResult Get([FromRoute(Name = "id")]int id)
        {
            //  Product product = _context.Products.First(p => p.Id.Equals(id));
            var model = _manager.PoductService.GetOneProduct(id, false);
            return View(model);
        }
    }
    
}