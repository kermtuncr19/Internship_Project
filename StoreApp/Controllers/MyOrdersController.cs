using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories;
namespace StoreApp.Controllers
{

    [Authorize]
    public class MyOrdersController : Controller
    {
        private readonly RepositoryContext _db;
        private readonly UserManager<IdentityUser> _um;

        public MyOrdersController(RepositoryContext db, UserManager<IdentityUser> um)
        { _db = db; _um = um; }

        public IActionResult Index()
        {
            var userId = _um.GetUserId(User)!;
            var orders = _db.Orders.Include(o => o.Lines).ThenInclude(l => l.Product).Where(o => o.UserId == userId)
                           .OrderByDescending(o => o.OrderedAt).ToList();
            return View(orders);
        }

        public IActionResult Detail(int id)
        {
            var userId = _um.GetUserId(User)!;
            var order = _db.Orders.Include(o => o.Lines).ThenInclude(l => l.Product).Where(o => o.UserId == userId && o.OrderId == id)
                          .FirstOrDefault();
            if (order == null) return NotFound();
            return View(order);
        }
    }

}