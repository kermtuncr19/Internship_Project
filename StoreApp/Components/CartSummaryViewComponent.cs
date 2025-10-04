using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace StoreApp.Components
{
    public class CartSummaryViewComponent : ViewComponent
    {
        private readonly Cart _cart;

        public CartSummaryViewComponent(Cart cartService)
        {
            _cart = cartService;
        }

        public IViewComponentResult Invoke()
        {
            // Toplam ürün adedi (aynı ürünün miktarıyla birlikte)
            var totalQuantity = _cart?.Lines?.Sum(l => l.Quantity) ?? 0;

            // Eğer “distinct ürün sayısı” göstermek istersen: var count = _cart.Lines.Count();
            return View(viewName: "Default", model: totalQuantity);
        }
    }
}
