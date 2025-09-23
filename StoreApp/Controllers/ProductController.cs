using Entities.Models;
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

        public IActionResult Index()
        {

            // var context = new RepositoryContext(
            //     new DbContextOptionsBuilder<RepositoryContext>()
            //     .UseSqlite("Data Source = C:\\Users\\kermtuncr\\Desktop\\Internship_Project\\ProductDb.db")
            //     .Options);  DI sayesinde buraya gerek kalmadı

            var model = _manager.PoductService.GetAllProducts(false);
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