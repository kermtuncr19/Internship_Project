using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Contracts;
using StoreApp.Infrastructure.Extensions;
using StoreApp.Models;
using System;
using System.Linq;
using Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace StoreApp.Pages
{
    public class CartModel : PageModel
    {
        private readonly IServiceManager _manager;
        private readonly RepositoryContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public Cart Cart { get; set; } // IoC
        public string ReturnUrl { get; set; } = "/";
        public AppliedCoupon? AppliedCoupon { get; set; }

        public CartModel(IServiceManager manager, Cart cartService, RepositoryContext db, UserManager<IdentityUser> userManager)
        {
            _manager = manager;
            Cart = cartService;
            _db = db;
            _userManager = userManager;
        }

        public void OnGet(string returnUrl)
        {
            ReturnUrl = returnUrl ?? "/";

            // ✅ Sepetteki ürünlerin stok durumunu kontrol et
            var stockWarnings = new List<string>();
            var outOfStockItems = new List<(int productId, string size)>();

            foreach (var line in Cart.Lines.ToList())
            {
                var stock = _manager.ProductStockService.GetStockByProductAndSize(
                    line.Product.ProductId,
                    line.Size
                );

                var availableQty = stock?.Quantity ?? 0;

                if (availableQty == 0)
                {
                    // Stok tükendi - sepetten çıkar
                    outOfStockItems.Add((line.Product.ProductId, line.Size));
                    var sizeText = string.IsNullOrEmpty(line.Size) ? "" : $" (Beden: {line.Size})";
                    stockWarnings.Add($"{line.Product.ProductName}{sizeText} stokta kalmadığı için sepetten çıkarıldı.");
                }
                else if (line.Quantity > availableQty)
                {
                    // Sepetteki miktar stoktan fazla - düzelt
                    line.Quantity = availableQty;
                    var sizeText = string.IsNullOrEmpty(line.Size) ? "" : $" (Beden: {line.Size})";
                    stockWarnings.Add($"{line.Product.ProductName}{sizeText} için miktar {availableQty} olarak güncellendi (stok yetersiz).");
                }
            }

            // Stok tükenen ürünleri sepetten çıkar
            foreach (var item in outOfStockItems)
            {
                var product = _manager.PoductService.GetOneProduct(item.productId, false);
                if (product != null)
                {
                    Cart.RemoveLine(product, item.size);
                }
            }

            if (stockWarnings.Any())
            {
                TempData["StockWarnings"] = stockWarnings;
            }

            AppliedCoupon = HttpContext.Session.GetJson<StoreApp.Models.AppliedCoupon>("applied_coupon");
        }

        // 🔹 Normal (full page) POST – istersen Product detaydan ekleme için kullanıyorsun
        public IActionResult OnPost(int productId, string returnUrl, string? size)
        {
            Product? product = _manager.PoductService.GetOneProduct(productId, false);

            if (product is null) return RedirectToPage(new { returnUrl });

            if (product.RequiresSize && string.IsNullOrWhiteSpace(size))
            {
                TempData["CartError"] = "Lütfen beden seçin.";
                return RedirectToAction("Get", "Product", new { id = productId });
            }

            // ✅ STOK KONTROLÜ
            var currentLine = Cart.Lines.FirstOrDefault(l =>
                l.Product.ProductId == productId && l.Size == size);
            var newQuantity = (currentLine?.Quantity ?? 0) + 1;

            var inStock = _manager.ProductStockService.IsInStock(productId, size, newQuantity);

            if (!inStock)
            {
                var stock = _manager.ProductStockService.GetStockByProductAndSize(productId, size);
                var availableQty = stock?.Quantity ?? 0;
                var sizeText = string.IsNullOrEmpty(size) ? "" : $" (Beden: {size})";

                TempData["CartError"] = $"Yetersiz stok{sizeText}. Mevcut: {availableQty} adet";
                return RedirectToAction("Get", "Product", new { id = productId });
            }

            Cart.AddItem(product, 1, size);
            return RedirectToPage(new { returnUrl });
        }

        public IActionResult OnPostRemove(int id, string returnUrl, string? size)
        {
            var product = _manager.PoductService.GetOneProduct(id, trackChanges: false);
            if (product is not null)
            {
                Cart.RemoveLine(product, size);
            }
            return RedirectToPage(new { returnUrl });
        }

        public IActionResult OnPostIncrement(int id, string returnUrl, string? size)
        {
            var product = _manager.PoductService.GetOneProduct(id, trackChanges: false);
            if (product is not null)
                Cart.AddItem(product, 1, size);

            return RedirectToPage(new { returnUrl });
        }

        public IActionResult OnPostDecrement(int id, string returnUrl, string? size)
        {
            var product = _manager.PoductService.GetOneProduct(id, trackChanges: false);
            if (product is not null)
                Cart.DecrementItem(product, size, 1);

            return RedirectToPage(new { returnUrl });
        }

        // ✅ Beden değiştirme (eski handler)
        public IActionResult OnPostChangeSize(int id, string oldSize, string newSize, string returnUrl)
        {
            if (Cart is SessionCart sessionCart)
            {
                sessionCart.ChangeSize(id, oldSize, newSize);
            }

            return RedirectToPage(new { returnUrl });
        }


        public async Task<IActionResult> OnPostApplyCoupon(string code)
        {
            code = (code ?? "").Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(code))
            {
                TempData["CouponError"] = "Kupon kodu boş olamaz.";
                return RedirectToPage("/cart");
            }

            var now = DateTime.UtcNow;
            var coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.Code == code);

            if (coupon == null)
            {
                TempData["CouponError"] = "Kupon bulunamadı.";
                return RedirectToPage("/cart");
            }

            if (!coupon.IsActive)
            {
                TempData["CouponError"] = "Kupon aktif değil.";
                return RedirectToPage("/cart");
            }

            if (coupon.StartsAtUtc.HasValue && now < coupon.StartsAtUtc.Value)
            {
                TempData["CouponError"] = "Kupon henüz başlamadı.";
                return RedirectToPage("/cart");
            }

            if (coupon.EndsAtUtc.HasValue && now > coupon.EndsAtUtc.Value)
            {
                TempData["CouponError"] = "Kupon süresi dolmuş.";
                return RedirectToPage("/cart");
            }

            if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
            {
                TempData["CouponError"] = "Kupon kullanım limiti dolmuş.";
                return RedirectToPage("/cart");
            }

            var cartTotal = Cart?.ComputeTotalValue() ?? 0m;
            if (coupon.MinCartTotal.HasValue && cartTotal < coupon.MinCartTotal.Value)
            {
                TempData["CouponError"] = $"Bu kupon için minimum sepet tutarı {coupon.MinCartTotal.Value.ToString("C2")}.";
                return RedirectToPage("/cart");
            }

            var userId = _userManager.GetUserId(User) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrWhiteSpace(userId))
            {
                var usedBefore = await _manager.CouponService.HasUserUsedAsync(userId, coupon.Code);
                if (usedBefore)
                {
                    TempData["CouponError"] = "Bu kuponu daha önce kullandınız.";
                    return RedirectToPage("/cart");
                }
            }

            // ✅ İndirimli ürün varsa kupon uygulanamaz
            var hasDiscountedItem = Cart?.Lines?.Any(l =>
                l.Product != null &&
                (
                    (l.Product.DiscountPercent > 0) ||
                    (l.Product.DiscountedPrice > 0 && l.Product.DiscountedPrice < l.Product.Price)
                )
            ) ?? false;

            if (hasDiscountedItem)
            {
                TempData["CouponError"] = "İndirimli ürün bulunan sepetlerde kupon kullanılamaz.";
                return RedirectToPage("/cart");
            }

            HttpContext.Session.SetJson("applied_coupon", new StoreApp.Models.AppliedCoupon
            {
                Code = coupon.Code,
                Percent = coupon.Percent
            });

            return RedirectToPage("/cart");
        }

        public IActionResult OnPostRemoveCoupon()
        {
            HttpContext.Session.Remove("applied_coupon");
            return RedirectToPage("/cart");
        }

        // ===================== AJAX KISMI =====================

        /// <summary>
        /// Ajax çağrılarından sonra frontende döneceğimiz ortak JSON
        /// </summary>
       private JsonResult BuildCartJson(int id, string? size)
{
    // ✅ İndirimsiz ara toplam
    decimal subtotalRaw = Cart?.Lines.Sum(l => l.Product.Price * l.Quantity) ?? 0m;

    // ✅ Ürün indirimli ara toplam
    decimal subtotalDiscounted = Cart?.Lines.Sum(l => l.Product.DiscountedPrice * l.Quantity) ?? 0m;

    // ✅ Ürün indirimi (adet dahil, satır satır garanti)
    decimal productDiscountAmount = Cart?.Lines
        .Sum(l => (l.Product.Price - l.Product.DiscountedPrice) * l.Quantity) ?? 0m;
    productDiscountAmount = Math.Max(0m, productDiscountAmount);

    // ✅ Kupon (indirimli toplam üzerinden)
    var applied = HttpContext.Session.GetJson<AppliedCoupon>("applied_coupon");
    decimal couponDiscountAmount = 0m;

    if (applied != null && applied.Percent > 0)
    {
        couponDiscountAmount = Math.Round(subtotalDiscounted * (applied.Percent / 100m), 2);
        if (couponDiscountAmount > subtotalDiscounted) couponDiscountAmount = subtotalDiscounted;
    }

    // ✅ Kupon sonrası toplam
    decimal total = subtotalDiscounted - couponDiscountAmount;

    // ✅ Kargo
    decimal shippingThreshold = 2000m;
    decimal shippingCost = total <= 0m ? 0m : (total >= shippingThreshold ? 0m : 39.99m);
    decimal grandTotal = total + shippingCost;
    decimal remaining = Math.Max(0m, shippingThreshold - total);

    // ✅ Satır (tıklanan ürün)
    var line = Cart.Lines.FirstOrDefault(l => l.Product.ProductId == id && l.Size == size);

    var lineTotalRaw = line != null ? line.Product.Price * line.Quantity : 0m;
    var lineTotalDiscounted = line != null ? line.Product.DiscountedPrice * line.Quantity : 0m;
    var hasLineDiscount = line != null && line.Product.DiscountPercent > 0;

    var progress = total <= 0m
        ? 0
        : (int)Math.Min(100m, Math.Round((total / shippingThreshold) * 100m, 0));

    return new JsonResult(new
    {
        success = true,

        // ================== SATIR ==================
        quantity = line?.Quantity ?? 0,
        removed = line == null,

        hasLineDiscount,
        lineDiscountPercent = line?.Product?.DiscountPercent ?? 0,

        lineTotalRaw,
        lineTotalRawFormatted = lineTotalRaw.ToString("C2"),

        lineTotalDiscounted,
        lineTotalDiscountedFormatted = lineTotalDiscounted.ToString("C2"),

        // (geriye dönük: eski kodların bozulmaması için)
        lineTotal = lineTotalDiscounted,
        lineTotalFormatted = lineTotalDiscounted.ToString("C2"),

        // ================== SIPARIS OZETI ==================
        subtotalRaw,
        subtotalRawFormatted = subtotalRaw.ToString("C2"),

        subtotalDiscounted,
        subtotalDiscountedFormatted = subtotalDiscounted.ToString("C2"),

        productDiscountAmount,
        productDiscountAmountFormatted = productDiscountAmount.ToString("C2"),

        couponCode = applied?.Code,
        couponPercent = applied?.Percent ?? 0,

        couponDiscountAmount,
        couponDiscountAmountFormatted = couponDiscountAmount.ToString("C2"),

        // geriye dönük (kupon için UI burada discountAmount bekliyorsa)
        discountAmount = couponDiscountAmount,
        discountAmountFormatted = couponDiscountAmount.ToString("C2"),

        // ================== TOPLAMLAR ==================
        cartTotal = total,
        cartTotalFormatted = total.ToString("C2"),

        shippingCost,
        shippingCostFormatted = shippingCost == 0m ? "Ücretsiz" : shippingCost.ToString("C2"),

        grandTotal,
        grandTotalFormatted = grandTotal.ToString("C2"),

        // ================== KARGO BAR ==================
        freeShipping = total >= shippingThreshold,
        progress,
        remainingFormatted = remaining.ToString("C2")
    });
}

        // 🔼 1 artır (AJAX)

        public JsonResult OnPostIncrementAjax(int id, string? size)
        {
            var product = _manager.PoductService.GetOneProduct(id, trackChanges: false);
            if (product is null)
                return new JsonResult(new { success = false, message = "Ürün bulunamadı" });

            // ✅ STOK KONTROLÜ
            var currentLine = Cart.Lines.FirstOrDefault(l =>
                l.Product.ProductId == id && l.Size == size);
            var newQuantity = (currentLine?.Quantity ?? 0) + 1;

            var inStock = _manager.ProductStockService.IsInStock(id, size, newQuantity);

            if (!inStock)
            {
                var stock = _manager.ProductStockService.GetStockByProductAndSize(id, size);
                var availableQty = stock?.Quantity ?? 0;

                return new JsonResult(new
                {
                    success = false,
                    message = $"Yetersiz stok. Mevcut: {availableQty} adet",
                    currentStock = availableQty
                });
            }

            Cart.AddItem(product, 1, size);
            return BuildCartJson(id, size);
        }

        // 🔽 1 azalt (AJAX)

        public JsonResult OnPostDecrementAjax(int id, string? size)
        {
            var product = _manager.PoductService.GetOneProduct(id, trackChanges: false);
            if (product is null)
                return new JsonResult(new { success = false });

            Cart.DecrementItem(product, size, 1);
            return BuildCartJson(id, size);
        }

        // 🗑 Sil (AJAX)

        public JsonResult OnPostRemoveAjax(int id, string? size)
        {
            var product = _manager.PoductService.GetOneProduct(id, trackChanges: false);
            if (product is not null)
            {
                Cart.RemoveLine(product, size);
            }

            return BuildCartJson(id, size);
        }
    }
}
