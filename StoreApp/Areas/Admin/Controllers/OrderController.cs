using Microsoft.AspNetCore.Mvc;
using Services.Contracts;

namespace StoreApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly IServiceManager _manager;

        public OrderController(IServiceManager manager)
        {
            _manager = manager;
        }

        public IActionResult Index()
        {
            var orders = _manager.OrderService.Orders;
            return View(orders);
        }
        [HttpPost]
        public IActionResult Complete([FromForm] int id)
        {
            _manager.OrderService.Complete(id);
            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult Cancel([FromForm] int id)
        {
            var order = _manager.OrderService.GetOneOrder(id);
            if (order != null)
            {
                order.Cancelled = true;
                _manager.OrderService.SaveOrder(order);
            }
            return RedirectToAction("Index");
        }


        [HttpPost]
        public IActionResult Delete([FromForm] int id)
        {
            _manager.OrderService.Delete(id);
            return RedirectToAction("Index");
        }

    }
}