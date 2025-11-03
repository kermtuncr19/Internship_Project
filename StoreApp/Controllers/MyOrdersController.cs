using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Entities.Models;
using StoreApp.Components;

namespace StoreApp.Controllers
{
    [Authorize]
    public class MyOrdersController : Controller
    {
        private readonly RepositoryContext _db;
        private readonly UserManager<IdentityUser> _um;

        public MyOrdersController(RepositoryContext db, UserManager<IdentityUser> um)
        {
            _db = db;
            _um = um;
        }

        public IActionResult Index()
{
    var userId = _um.GetUserId(User)!;

    var orders = _db.Orders
        .Include(o => o.Lines)
            .ThenInclude(l => l.Product)
        .Include(o => o.ReturnRequests)
            .ThenInclude(r => r.Lines)
                .ThenInclude(rl => rl.CartLine)
                    .ThenInclude(cl => cl.Product)
        .Where(o => o.UserId == userId)
        .OrderByDescending(o => o.OrderedAt)
        .AsSplitQuery() // büyük join’lerde şişmeyi azaltır
        .ToList();

    return View(orders);
}


        public IActionResult Detail(int id)
        {
            var userId = _um.GetUserId(User)!;

            // 🔴 Satır bazında iade durumu rozetleri için CartLine include edildi
            var order = _db.Orders
                .Include(o => o.Lines).ThenInclude(l => l.Product)
                .Include(o => o.ReturnRequests)
                    .ThenInclude(r => r.Lines)
                        .ThenInclude(rl => rl.CartLine)
                .Where(o => o.UserId == userId && o.OrderId == id)
                .FirstOrDefault();

            if (order == null)
            {
                TempData["error"] = "Sipariş bulunamadı veya bu siparişi görüntüleme yetkiniz yok.";
                return RedirectToAction("Index");
            }

            return View(order);
        }

        // Sipariş İptal Etme (Sadece Hazırlanıyor adımından önce)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder(int id, string reason)
        {
            var userId = _um.GetUserId(User)!;
            var order = _db.Orders.FirstOrDefault(o => o.OrderId == id && o.UserId == userId);

            if (order == null)
                return Json(new { success = false, message = "Sipariş bulunamadı." });

            // İptal edilebilirlik kontrolleri
            if (order.Cancelled || order.CancelledByUser)
                return Json(new { success = false, message = "Bu sipariş zaten iptal edilmiş." });

            if (order.Preparing || order.InTransit || order.Delivered)
                return Json(new { success = false, message = "Sipariş hazırlanma aşamasına geçtiği için iptal edilemez." });

            if (string.IsNullOrWhiteSpace(reason))
                return Json(new { success = false, message = "Lütfen iptal nedenini belirtiniz." });

            // İptal işlemi
            order.CancelledByUser = true;
            order.CancelledByUserAt = DateTime.UtcNow;
            order.CancellationReason = reason;
            order.Cancelled = true;
            order.CancelledAt = DateTime.UtcNow;

            _db.SaveChanges();

            return Json(new { success = true, message = "Siparişiniz başarıyla iptal edildi." });
        }

        // İade Talebi Sayfası (Ürün Seçimi)
     [HttpGet]
public IActionResult Return(int id)
{
    var userId = _um.GetUserId(User)!;

    var order = _db.Orders
        .Include(o => o.Lines).ThenInclude(l => l.Product)
        .Include(o => o.ReturnRequests).ThenInclude(r => r.Lines)
        .FirstOrDefault(o => o.OrderId == id && o.UserId == userId);

    if (order == null)
    {
        TempData["error"] = "Sipariş bulunamadı.";
        return RedirectToAction("Index");
    }
    if (!order.Delivered)
    {
        TempData["error"] = "Sadece teslim edilmiş siparişler iade edilebilir.";
        return RedirectToAction("Detail", new { id });
    }
    if (order.Cancelled)
    {
        TempData["error"] = "İptal edilmiş siparişler iade edilemez.";
        return RedirectToAction("Detail", new { id });
    }

    var returnDeadline = order.DeliveredAt?.AddDays(15);
    if (!returnDeadline.HasValue || DateTime.UtcNow > returnDeadline.Value)
    {
        TempData["error"] = "İade süresi dolmuştur. Siparişler teslim tarihinden itibaren 15 gün içinde iade edilebilir.";
        return RedirectToAction("Detail", new { id });
    }

    // --- Her CartLineId için (statüsü ne olursa olsun) iade statüsünü çıkar ---
    // En “güncel” talep baz alınsın diye ReturnRequestId desc sıralayıp first alıyoruz
    var lineStatusMap = order.ReturnRequests
        .OrderByDescending(r => r.ReturnRequestId)
        .SelectMany(r => r.Lines.Select(rl => new
        {
            rl.CartLineId,
            r.Status,
            r.AdminNotes,
            r.ProcessedAt
        }))
        .GroupBy(x => x.CartLineId)
        .ToDictionary(
            g => g.Key,
            g => g.First() // en yeni talep
        );

    var locked = lineStatusMap.Keys.ToHashSet();

    var vm = new ReturnRequestViewModel
    {
        Order = order,
        ReturnDeadline = returnDeadline.Value,
        LockedLineIds = locked,
        // Aşağıdaki üç sözlük view'da statüye göre mesaj basmak için:
        LineStatus = lineStatusMap.ToDictionary(k => k.Key, v => v.Value.Status),
        LineProcessedAt = lineStatusMap.ToDictionary(k => k.Key, v => v.Value.ProcessedAt),
        LineAdminNotes = lineStatusMap.ToDictionary(k => k.Key, v => v.Value.AdminNotes)
    };

    return View(vm);
}



     // İade Talebini Kaydet
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult ProcessReturn(int orderId, List<int> selectedLines, string reason, string detailedReason)
{
    var userId = _um.GetUserId(User)!;

    var order = _db.Orders
        .Include(o => o.Lines).ThenInclude(l => l.Product)
        .Include(o => o.ReturnRequests).ThenInclude(r => r.Lines)
        .FirstOrDefault(o => o.OrderId == orderId && o.UserId == userId);

    if (order == null || !order.Delivered)
        return Json(new { success = false, message = "Geçersiz sipariş." });

    if (selectedLines == null || !selectedLines.Any())
        return Json(new { success = false, message = "Lütfen en az bir ürün seçiniz." });

    if (string.IsNullOrWhiteSpace(reason))
        return Json(new { success = false, message = "Lütfen iade nedenini belirtiniz." });

    var returnDeadline = order.DeliveredAt?.AddDays(15);
    if (!returnDeadline.HasValue || DateTime.UtcNow > returnDeadline.Value)
        return Json(new { success = false, message = "İade süresi dolmuştur." });

    // *** KRİTİK BLOKAJ: Seçilen satırlardan herhangi birinin daha önce iade talebi varsa (statüsü ne olursa olsun) izin verme
    var anyExisting = _db.ReturnRequestLines
        .Include(l => l.ReturnRequest)
        .Any(l => selectedLines.Contains(l.CartLineId)
                  && l.ReturnRequest.OrderId == orderId
                  && l.ReturnRequest.UserId == userId);

    if (anyExisting)
        return Json(new { success = false, message = "Seçtiğiniz ürünlerden bazıları için daha önce iade talebi oluşturulmuş." });

    var rr = new ReturnRequest
    {
        OrderId = orderId,
        UserId = userId,
        RequestedAt = DateTime.UtcNow,
        Reason = reason,
        DetailedReason = detailedReason,
        Status = ReturnStatus.Pending,
        Lines = new List<ReturnRequestLine>()
    };

    foreach (var lineId in selectedLines)
    {
        var cartLine = order.Lines.FirstOrDefault(l => l.CartLineId == lineId);
        if (cartLine != null)
        {
            rr.Lines.Add(new ReturnRequestLine
            {
                CartLineId = lineId,
                Quantity = cartLine.Quantity
            });
        }
    }

    _db.ReturnRequests.Add(rr);
    _db.SaveChanges();

    return Json(new { success = true, message = "İade talebiniz oluşturuldu." });
}


        // İade Taleplerim
        public IActionResult MyReturns()
        {
            var userId = _um.GetUserId(User)!;
            var returns = _db.ReturnRequests
                .Include(r => r.Order)
                .Include(r => r.Lines).ThenInclude(l => l.CartLine).ThenInclude(c => c.Product)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RequestedAt)
                .ToList();

            return View(returns);
        }
    }
}
