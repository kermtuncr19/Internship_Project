using System;
using System.Linq;
using System.Security.Claims; // ⬅️ UserId için
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services.Contracts;

namespace StoreApp.Controllers
{
    [Authorize] // hem GET hem POST login gerektirsin
    public class OrderController : Controller
    {
        private readonly IServiceManager _manager;
        private readonly Cart _cart;
        private readonly UserManager<IdentityUser> _userManager;

        public OrderController(IServiceManager manager, Cart cart, UserManager<IdentityUser> userManager)
        {
            _manager = manager;
            _cart = cart;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Checkout()
        {
            if (!_cart.Lines.Any())
            {
                TempData["CartEmpty"] = "Sepetiniz boş. Ödeme işlemine devam edemezsiniz.";
                return RedirectToPage("/cart");
            }

            // Boş form (adres seçimleri JS ile dolduruluyor)
            return View(new Order());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout([FromForm] Order order)
        {
            // 1) Sepet boşsa
            if (!_cart.Lines.Any())
            {
                ModelState.AddModelError(string.Empty, "Üzgünüm, sepetiniz boş.");
            }

            // 2) Basit alan doğrulamaları (City/District/Neighborhood/Address gibi)
            // Eğer Order modelinde bu alanları zorunlu yaptınızsa, bunlar zaten ModelState üzerinde olur.
            // Yine de güvene almak için kısa kontroller:
            if (string.IsNullOrWhiteSpace(order.Name))
                ModelState.AddModelError(nameof(order.Name), "İsim gerekli.");

            if (string.IsNullOrWhiteSpace(order.City))
                ModelState.AddModelError(nameof(order.City), "Şehir seçiniz.");

            if (string.IsNullOrWhiteSpace(order.District))
                ModelState.AddModelError(nameof(order.District), "İlçe seçiniz.");

            if (string.IsNullOrWhiteSpace(order.Neighborhood))
                ModelState.AddModelError(nameof(order.Neighborhood), "Mahalle seçiniz.");

            if (string.IsNullOrWhiteSpace(order.Address))
                ModelState.AddModelError(nameof(order.Address), "Açık adres giriniz.");

            if (!ModelState.IsValid)
                return View(order);

            // 3) Siparişi oturumdaki kullanıcıya bağla + zaman
            order.UserId = _userManager.GetUserId(User) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            order.OrderedAt = DateTime.UtcNow;

            // 4) Sepet satırlarını sadece gerekli alanlarla kopyala
            //    (CartLine entity'nizde ProductId/OrderId/Quantity/Size alanları olmalı; Product navigation'ını set etmiyoruz)
            order.Lines = _cart.Lines.Select(l => new CartLine
            {
                ProductId = l.Product.ProductId, // ❗ ÖNEMLİ: FK
                Quantity = l.Quantity,
                Size = l.Size,
                UnitPrice =l.Product.Price
            }).ToList();

            // 5) Kaydet
            _manager.OrderService.SaveOrder(order);

            // 6) Sepeti temizle
            _cart.Clear();

            // 7) Tamamlama ekranına yönlendir (Razor Page ise)
            return RedirectToPage("/Complete", new { OrderId = order.OrderId });

            // MVC View kullanıyorsanız alternatif:
            // return RedirectToAction("Details", "MyOrders", new { id = order.OrderId });
        }
    }
}
