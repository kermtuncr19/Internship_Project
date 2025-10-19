using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Contracts;

namespace StoreApp.Controllers
{
    public class OrderController : Controller
    {
        private readonly IServiceManager _manager;
        private readonly Cart _cart;

        public OrderController(IServiceManager manager, Cart cart)
        {
            _manager = manager;
            _cart = cart;
        }
        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {
            if (!_cart.Lines.Any())
            {
                TempData["CartEmpty"] = "Sepetiniz boş. Ödeme işlemine devam edemezsiniz.";
                return RedirectToPage("/cart");
            }
            return View(new Order());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout([FromForm] Order order)
        {
            if (_cart.Lines.Count() == 0)
            {
                ModelState.AddModelError("", "Üzgünüm, Sepetiniz Boş.");
            }
            if (ModelState.IsValid)
            {
                order.Lines = _cart.Lines.ToArray();
                _manager.OrderService.SaveOrder(order);
                _cart.Clear();
                return RedirectToPage("/Complete", new { OrderId = order.OrderId });
            }
            else
            {
                return View();
            }
        }
    }
}