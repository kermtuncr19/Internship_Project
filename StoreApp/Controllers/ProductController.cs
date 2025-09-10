using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreApp.Models;

namespace StoreApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly RepositoryContext _context;//dependency injection

        public ProductController(RepositoryContext context)//dependency injection
        {
            _context = context;
        }

        public IActionResult Index()
        {

            // var context = new RepositoryContext(
            //     new DbContextOptionsBuilder<RepositoryContext>()
            //     .UseSqlite("Data Source = C:\\Users\\kermtuncr\\Desktop\\Internship_Project\\ProductDb.db")
            //     .Options);  DI sayesinde buraya gerek kalmadı

            var model = _context.Products.ToList();
            return View(model);
        }

        public IActionResult Get(int id)
        {
            Product product = _context.Products.First(p => p.Id.Equals(id));
            return View(product);
        }
    }
    
}