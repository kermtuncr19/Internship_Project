using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services.Contracts;
using StoreApp.Infrastructure.Extensions;
using StoreApp.Models;

namespace StoreApp.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IServiceManager _manager;
        private readonly Cart _cart;
        private readonly UserManager<IdentityUser> _userManager;

        public OrderController(
            IServiceManager manager,
            Cart cart,
            UserManager<IdentityUser> userManager)
        {
            _manager = manager;
            _cart = cart;
            _userManager = userManager;
        }

        // ADIM 1: Checkout Sayfası (Adres Bilgileri)
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            if (!_cart.Lines.Any())
            {
                TempData["CartEmpty"] = "Sepetiniz boş. Ödeme işlemine devam edemezsiniz.";
                return RedirectToPage("/cart");
            }

            var userId = _userManager.GetUserId(User);

            var viewModel = new CheckoutViewModel
            {
                Order = new Order(),
                SavedAddresses = await _manager.AddressService.GetAllAsync(userId)
            };

            return View(viewModel);
        }

        // ADIM 2: Adres Bilgilerini Kaydet ve Ödeme Sayfasına Yönlendir
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout([FromForm] Order order)
        {
            // 1) Sepet boşsa
            if (!_cart.Lines.Any())
            {
                ModelState.AddModelError(string.Empty, "Üzgünüm, sepetiniz boş.");
            }

            // 2) Basit alan doğrulamaları
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
            {
                var userId = _userManager.GetUserId(User);
                var viewModel = new CheckoutViewModel
                {
                    Order = order,
                    SavedAddresses = await _manager.AddressService.GetAllAsync(userId)
                };
                return View(viewModel);
            }

            // 3) Order bilgilerini session'a kaydet
            order.UserId = _userManager.GetUserId(User) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            HttpContext.Session.SetJson("PendingOrder", order);

            // 4) Ödeme sayfasına yönlendir
            return RedirectToAction(nameof(Payment));
        }

        // ADIM 3: Ödeme Sayfası
        [HttpGet]
        public IActionResult Payment()
        {
            // Session'dan order bilgilerini al
            var order = HttpContext.Session.GetJson<Order>("PendingOrder");

            if (order == null || !_cart.Lines.Any())
            {
                TempData["Error"] = "Sipariş bilgileri bulunamadı. Lütfen tekrar deneyin.";
                return RedirectToAction(nameof(Checkout));
            }

            // Sepet satırlarını Product bilgileriyle birlikte kopyala (gösterim için)
            var cartItems = _cart.Lines.Select(l => new CartLine
            {
                Product = l.Product,
                Quantity = l.Quantity,
                Size = l.Size,
                UnitPrice = l.Product.Price
            }).ToList();

            var viewModel = new PaymentViewModel
            {
                Order = order,
                PaymentInfo = new PaymentInfo(),
                TotalAmount = _cart.Lines.Sum(l => l.Product.Price * l.Quantity),
                CartItems = cartItems
            };

            return View(viewModel);
        }

        // ADIM 4: Ödeme İşlemi ve Sipariş Oluşturma
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProcessPayment([FromForm] PaymentInfo paymentInfo)
        {
            var order = HttpContext.Session.GetJson<Order>("PendingOrder");

            if (order == null || !_cart.Lines.Any())
            {
                TempData["Error"] = "Sipariş bilgileri bulunamadı. Lütfen tekrar deneyin.";
                return RedirectToAction(nameof(Checkout));
            }

            var instStr = Request.Form["Installment"].ToString();
            if (int.TryParse(instStr, out var taksit) && taksit > 0)
            {
                TempData["Installment"] = taksit;
                order.Installment = taksit;
            }

            // ✅ STOK KONTROLÜ - Ödeme öncesi
            var stockErrors = new List<string>();
            foreach (var line in _cart.Lines)
            {
                var inStock = _manager.ProductStockService.IsInStock(
                    line.Product.ProductId,
                    line.Size,
                    line.Quantity
                );

                if (!inStock)
                {
                    var stock = _manager.ProductStockService.GetStockByProductAndSize(
                        line.Product.ProductId,
                        line.Size
                    );

                    var availableQty = stock?.Quantity ?? 0;
                    var sizeText = string.IsNullOrEmpty(line.Size) ? "" : $" (Beden: {line.Size})";

                    stockErrors.Add($"{line.Product.ProductName}{sizeText} - Mevcut stok: {availableQty}, İstenen: {line.Quantity}");
                }
            }

            if (stockErrors.Any())
            {
                TempData["Error"] = "Bazı ürünlerde yeterli stok yok:\n" + string.Join("\n", stockErrors);
                return RedirectToPage("/cart");
            }

            // Payment validation
            if (!ModelState.IsValid)
            {
                var cartItems = _cart.Lines.Select(l => new CartLine
                {
                    Product = l.Product,
                    Quantity = l.Quantity,
                    Size = l.Size,
                    UnitPrice = l.Product.Price
                }).ToList();

                var viewModel = new PaymentViewModel
                {
                    Order = order,
                    PaymentInfo = paymentInfo,
                    TotalAmount = _cart.Lines.Sum(l => l.Product.Price * l.Quantity),
                    CartItems = cartItems
                };
                return View("Payment", viewModel);
            }

            // Yapay ödeme işlemi
            bool paymentSuccessful = ProcessFakePayment(paymentInfo);

            if (!paymentSuccessful)
            {
                ModelState.AddModelError(string.Empty, "Ödeme işlemi başarısız. Lütfen kart bilgilerinizi kontrol edin.");

                var cartItems = _cart.Lines.Select(l => new CartLine
                {
                    Product = l.Product,
                    Quantity = l.Quantity,
                    Size = l.Size,
                    UnitPrice = l.Product.Price
                }).ToList();

                var viewModel = new PaymentViewModel
                {
                    Order = order,
                    PaymentInfo = paymentInfo,
                    TotalAmount = _cart.Lines.Sum(l => l.Product.Price * l.Quantity),
                    CartItems = cartItems
                };
                return View("Payment", viewModel);
            }

            // ✅ ÖDEME BAŞARILI - STOKLARI AZALT
            try
            {
                foreach (var line in _cart.Lines)
                {
                    _manager.ProductStockService.DecreaseStock(
                        line.Product.ProductId,
                        line.Size,
                        line.Quantity
                    );
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Stok güncellenirken hata oluştu: " + ex.Message;
                return RedirectToPage("/cart");
            }

            // Siparişi oluştur
            order.OrderedAt = DateTime.UtcNow;
            order.Lines = _cart.Lines.Select(l => new CartLine
            {
                ProductId = l.Product.ProductId,
                Quantity = l.Quantity,
                Size = l.Size,
                UnitPrice = l.Product.Price
            }).ToList();

            // Siparişi kaydet
            _manager.OrderService.SaveOrder(order);

            // Sepeti temizle
            _cart.Clear();

            // Session'dan pending order'ı sil
            HttpContext.Session.Remove("PendingOrder");

            // Başarı sayfasına yönlendir
            TempData["OrderSuccess"] = "Ödemeniz başarıyla alındı!";
            return RedirectToPage("/Complete", new { OrderId = order.OrderId });
        }

        // Yapay ödeme işlemi
        private bool ProcessFakePayment(PaymentInfo paymentInfo)
        {
            // Basit validasyon: Kart numarası 4 ile başlıyorsa başarılı
            // Gerçek uygulamada burada payment gateway API'sine istek atılır

            // Simülasyon: %90 başarı oranı
            var random = new Random();
            var success = paymentInfo.CardNumber.StartsWith("4") && random.Next(100) < 95;

            // Gerçek hayatta burası şöyle olurdu:
            // var result = await _paymentGateway.ProcessPayment(paymentInfo, amount);
            // return result.IsSuccessful;

            // 2 saniye bekleme simülasyonu (ödeme işlemi)
            System.Threading.Thread.Sleep(2000);

            return success;
        }
    }
}