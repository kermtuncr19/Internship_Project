using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Contracts;
using StoreApp.Infrastructure.Extensions;
using StoreApp.Models;
using System;
using System.Linq;

namespace StoreApp.Pages
{
    public class CartModel : PageModel
    {
        private readonly IServiceManager _manager;

        public Cart Cart { get; set; } // IoC
        public string ReturnUrl { get; set; } = "/";

        public CartModel(IServiceManager manager, Cart cartService)
        {
            _manager = manager;
            Cart = cartService;
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

        // ===================== AJAX KISMI =====================

        /// <summary>
        /// Ajax çağrılarından sonra frontende döneceğimiz ortak JSON
        /// </summary>
        private JsonResult BuildCartJson(int id, string? size)
        {
            decimal total = Cart?.ComputeTotalValue() ?? 0m;
            decimal shippingThreshold = 2000m;
            decimal shippingCost = total <= 0m ? 0m : (total >= shippingThreshold ? 0m : 39.99m);
            decimal grandTotal = total + shippingCost;
            decimal remaining = Math.Max(0m, shippingThreshold - total);

            var line = Cart.Lines
                .FirstOrDefault(l => l.Product.ProductId == id && l.Size == size);

            var lineTotal = line != null ? line.Product.Price * line.Quantity : 0m;

            var progress = total <= 0m
                ? 0
                : (int)Math.Min(100m, Math.Round((total / shippingThreshold) * 100m, 0));

            return new JsonResult(new
            {
                success = true,

                // satır bilgileri
                quantity = line?.Quantity ?? 0,
                lineTotal,
                lineTotalFormatted = lineTotal.ToString("C2"),
                removed = line == null,

                // sepet toplamları
                cartTotal = total,
                cartTotalFormatted = total.ToString("C2"),
                shippingCost,
                shippingCostFormatted = shippingCost == 0m ? "Ücretsiz" : shippingCost.ToString("C2"),
                grandTotal,
                grandTotalFormatted = grandTotal.ToString("C2"),

                // kargo barı
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
