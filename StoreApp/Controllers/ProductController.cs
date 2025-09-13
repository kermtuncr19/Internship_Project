using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Repositories.Contracts;


namespace StoreApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly IRepositoryManager _manager;//dependency injection

        public ProductController(IRepositoryManager manager)
        {
            _manager = manager;
        }

        public IActionResult Index()
        {

            // var context = new RepositoryContext(
            //     new DbContextOptionsBuilder<RepositoryContext>()
            //     .UseSqlite("Data Source = C:\\Users\\kermtuncr\\Desktop\\Internship_Project\\ProductDb.db")
            //     .Options);  DI sayesinde buraya gerek kalmadı

            var model = _manager.Product.GetAllProducts(false);
            return View(model);
        }

        public IActionResult Get(int id)
        {
            //  Product product = _context.Products.First(p => p.Id.Equals(id));
            var model = _manager.Product.GetOneProduct(id, false);
            return View(model);
        }
    }
    
}