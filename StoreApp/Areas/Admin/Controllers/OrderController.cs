using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace StoreApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly IServiceManager _manager;

        public OrderController(IServiceManager manager)
        {
            _manager = manager;
        }

        public IActionResult Index()
        {
            // ReturnRequests'i de Include et
            var orders = _manager.OrderService.Orders
                .Include(o => o.ReturnRequests)
                    .ThenInclude(r => r.Lines)
                        .ThenInclude(l => l.CartLine)
                .OrderByDescending(o => o.OrderedAt)
                .ToList();
                
            return View(orders);
        }

        // Detail action - ReturnRequests ile birlikte getir
        public IActionResult Detail(int id)
        {
            var order = _manager.OrderService.Orders
                .Include(o => o.ReturnRequests)
                    .ThenInclude(r => r.Lines)
                        .ThenInclude(l => l.CartLine)
                            .ThenInclude(c => c.Product)
                .FirstOrDefault(o => o.OrderId == id);
            
            if (order == null)
            {
                TempData["error"] = "Sipariş bulunamadı.";
                return RedirectToAction("Index");
            }
            
            return View("_OrderDetails", order);
        }

        [HttpPost]
        public IActionResult Complete([FromForm] int id)
        {
            var order = _manager.OrderService.GetOneOrder(id);
            if (order == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Sipariş bulunamadı." });
                TempData["error"] = "Sipariş bulunamadı.";
                return RedirectToAction("Index");
            }
            if (order.Cancelled)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "İptal edilmiş sipariş onaylanamaz." });
                TempData["error"] = "İptal edilmiş sipariş onaylanamaz.";
                return RedirectToAction("Index");
            }
            order.Shipped = true;
            order.ShippedAt = DateTime.UtcNow;
            _manager.OrderService.SaveOrder(order);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, message = "Sipariş onaylandı." });
            TempData["success"] = "Sipariş onaylandı.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult MarkAsPreparing([FromForm] int id)
        {
            var order = _manager.OrderService.GetOneOrder(id);
            if (order == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Sipariş bulunamadı." });
                TempData["error"] = "Sipariş bulunamadı.";
                return RedirectToAction("Index");
            }
            if (!order.Shipped)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Önce siparişi onaylamalısınız." });
                TempData["error"] = "Önce siparişi onaylamalısınız.";
                return RedirectToAction("Index");
            }
            if (order.Cancelled)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "İptal edilmiş sipariş güncellenemez." });
                TempData["error"] = "İptal edilmiş sipariş güncellenemez.";
                return RedirectToAction("Index");
            }
            order.Preparing = true;
            order.PreparingAt = DateTime.UtcNow;
            _manager.OrderService.SaveOrder(order);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, message = "Sipariş 'Hazırlanıyor' olarak işaretlendi." });
            TempData["success"] = "Sipariş 'Hazırlanıyor' olarak işaretlendi.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult MarkAsInTransit([FromForm] int id)
        {
            var order = _manager.OrderService.GetOneOrder(id);
            if (order == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Sipariş bulunamadı." });
                TempData["error"] = "Sipariş bulunamadı.";
                return RedirectToAction("Index");
            }
            if (!order.Preparing)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Sipariş önce hazırlanıyor durumuna alınmalı." });
                TempData["error"] = "Sipariş önce hazırlanıyor durumuna alınmalı.";
                return RedirectToAction("Index");
            }
            if (order.Cancelled)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "İptal edilmiş sipariş güncellenemez." });
                TempData["error"] = "İptal edilmiş sipariş güncellenemez.";
                return RedirectToAction("Index");
            }
            order.InTransit = true;
            order.InTransitAt = DateTime.UtcNow;
            _manager.OrderService.SaveOrder(order);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, message = "Sipariş kargoya verildi olarak işaretlendi." });
            TempData["success"] = "Sipariş kargoya verildi olarak işaretlendi.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult MarkAsDelivered([FromForm] int id)
        {
            var order = _manager.OrderService.GetOneOrder(id);
            if (order == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Sipariş bulunamadı." });
                TempData["error"] = "Sipariş bulunamadı.";
                return RedirectToAction("Index");
            }
            if (!order.InTransit)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Sipariş önce kargoya verilmiş olmalı." });
                TempData["error"] = "Sipariş önce kargoya verilmiş olmalı.";
                return RedirectToAction("Index");
            }
            if (order.Cancelled)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "İptal edilmiş sipariş güncellenemez." });
                TempData["error"] = "İptal edilmiş sipariş güncellenemez.";
                return RedirectToAction("Index");
            }
            order.Delivered = true;
            order.DeliveredAt = DateTime.UtcNow;
            _manager.OrderService.SaveOrder(order);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, message = "Sipariş teslim edildi olarak işaretlendi. Kullanıcı artık yorum yapabilir." });
            TempData["success"] = "Sipariş teslim edildi olarak işaretlendi. Kullanıcı artık yorum yapabilir.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Cancel([FromForm] int id)
        {
            var order = _manager.OrderService.GetOneOrder(id);
            if (order == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Sipariş bulunamadı." });
                TempData["error"] = "Sipariş bulunamadı.";
                return RedirectToAction("Index");
            }
            if (order.Delivered)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Teslim edilmiş sipariş iptal edilemez." });
                TempData["error"] = "Teslim edilmiş sipariş iptal edilemez.";
                return RedirectToAction("Index");
            }
            order.Cancelled = true;
            order.CancelledAt = DateTime.UtcNow;
            _manager.OrderService.SaveOrder(order);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, message = "Sipariş başarıyla iptal edildi." });
            TempData["success"] = "Sipariş başarıyla iptal edildi.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete([FromForm] int id)
        {
            try
            {
                _manager.OrderService.Delete(id);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = true, message = "Sipariş silindi." });

                TempData["success"] = "Sipariş silindi.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Silme sırasında hata: " + ex.Message });

                TempData["error"] = "Silme sırasında bir hata oluştu.";
                return RedirectToAction("Index");
            }
        }
    }
}