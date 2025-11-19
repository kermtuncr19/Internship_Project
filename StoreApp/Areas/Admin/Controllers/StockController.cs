using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Contracts;

namespace StoreApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class StockController : Controller
    {
        private readonly IServiceManager _manager;

        public StockController(IServiceManager manager)
        {
            _manager = manager;
        }

        // Ürün stok yönetim sayfası
        public IActionResult Manage(int id)
        {
            var product = _manager.PoductService.GetOneProduct(id, false);
            if (product == null)
                return NotFound();

            var stocks = _manager.ProductStockService.GetStocksByProductId(id);
            
            ViewBag.Product = product;
            ViewBag.RequiresSize = product.RequiresSize;
            
            // Eğer bedeni varsa, mevcut bedenleri al
            if (product.RequiresSize && !string.IsNullOrWhiteSpace(product.SizeOptionsCsv))
            {
                ViewBag.AvailableSizes = product.SizeOptionsCsv
                    .Split(',')
                    .Select(s => s.Trim())
                    .ToList();
            }
            
            return View(stocks);
        }

        // Stok ekleme/güncelleme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStock(int productId, string? size, int quantity)
        {
            try
            {
                _manager.ProductStockService.CreateOrUpdateStock(productId, size, quantity);
                TempData["success"] = "Stok başarıyla güncellendi.";
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Hata: {ex.Message}";
            }
            
            return RedirectToAction("Manage", new { id = productId });
        }

        // Stok silme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteStock(int stockId, int productId)
        {
            try
            {
                _manager.ProductStockService.DeleteStock(stockId);
                TempData["success"] = "Stok kaydı silindi.";
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Hata: {ex.Message}";
            }
            
            return RedirectToAction("Manage", new { id = productId });
        }

        // Tüm bedenlere aynı stoğu atama
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetAllSizesStock(int productId, int quantity)
        {
            try
            {
                var product = _manager.PoductService.GetOneProduct(productId, false);
                
                if (product.RequiresSize && !string.IsNullOrWhiteSpace(product.SizeOptionsCsv))
                {
                    var sizes = product.SizeOptionsCsv.Split(',').Select(s => s.Trim());
                    
                    foreach (var size in sizes)
                    {
                        _manager.ProductStockService.CreateOrUpdateStock(productId, size, quantity);
                    }
                    
                    TempData["success"] = $"Tüm bedenlere {quantity} adet stok atandı.";
                }
                else
                {
                    TempData["error"] = "Bu ürün beden gerektirmiyor.";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Hata: {ex.Message}";
            }
            
            return RedirectToAction("Manage", new { id = productId });
        }
    }
}