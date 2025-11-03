using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Entities.Models;

namespace StoreApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReturnRequestController : Controller
    {
        private readonly RepositoryContext _db;
        private readonly UserManager<IdentityUser> _um;

        public ReturnRequestController(RepositoryContext db, UserManager<IdentityUser> um)
        {
            _db = db;
            _um = um;
        }

        // İade Talepleri Listesi (filtre + sayfalama)
        // /Admin/ReturnRequest?status=Pending&orderId=123&email=a@b.com&page=1&pageSize=20
        public async Task<IActionResult> Index(string? status, int? orderId, string? email, int page = 1, int pageSize = 20)
        {
            var q = _db.ReturnRequests
                .Include(r => r.Order)
                .Include(r => r.Lines).ThenInclude(l => l.CartLine).ThenInclude(c => c.Product)
                .OrderByDescending(r => r.RequestedAt)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ReturnStatus>(status, out var parsed))
                q = q.Where(r => r.Status == parsed);

            if (orderId.HasValue)
                q = q.Where(r => r.OrderId == orderId.Value);

            if (!string.IsNullOrWhiteSpace(email))
            {
                // AspNetUsers üzerinden mail filtresi
                var userIds = _db.Users
                    .Where(u => u.Email != null && u.Email.Contains(email))
                    .Select(u => u.Id)
                    .ToList();
                q = q.Where(r => r.UserId != null && userIds.Contains(r.UserId));
            }

            var total = await q.CountAsync();
            var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var vm = new AdminReturnRequestListVm
            {
                Items = items,
                Status = status,
                OrderId = orderId,
                Email = email,
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            };
            return View(vm);
        }

        // Belirli siparişin taleplerini göster (tek ise direkt detayına yönlendir)
        public IActionResult ListByOrder(int orderId)
        {
            var returns = _db.ReturnRequests
                .Include(r => r.Order)
                .Include(r => r.Lines).ThenInclude(l => l.CartLine).ThenInclude(c => c.Product)
                .Where(r => r.OrderId == orderId)
                .OrderByDescending(r => r.RequestedAt)
                .ToList();

            if (!returns.Any())
            {
                TempData["error"] = $"#{orderId} numaralı sipariş için iade talebi bulunamadı.";
                return RedirectToAction("Detail", "Order", new { area = "Admin", id = orderId });
            }

            if (returns.Count == 1)
                return RedirectToAction("Detail", new { id = returns[0].ReturnRequestId });

            // Listeyi genel Index görünümüyle render et
            var vm = new AdminReturnRequestListVm
            {
                Items = returns,
                OrderId = orderId,
                Page = 1,
                PageSize = returns.Count,
                TotalCount = returns.Count
            };
            return View("Index", vm);
        }

        // Talep detay
        public IActionResult Detail(int id)
        {
            var returnRequest = _db.ReturnRequests
                .Include(r => r.Order).ThenInclude(o => o.Lines).ThenInclude(l => l.Product)
                .Include(r => r.Lines).ThenInclude(l => l.CartLine).ThenInclude(c => c.Product)
                .FirstOrDefault(r => r.ReturnRequestId == id);

            if (returnRequest == null)
            {
                TempData["error"] = "İade talebi bulunamadı.";
                return RedirectToAction("Index");
            }

            return View(returnRequest);
        }

        // Onayla
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int id, string adminNotes)
        {
            var returnRequest = _db.ReturnRequests.FirstOrDefault(r => r.ReturnRequestId == id);
            if (returnRequest == null)
                return Json(new { success = false, message = "İade talebi bulunamadı." });

            if (returnRequest.Status != ReturnStatus.Pending)
                return Json(new { success = false, message = "Bu talep zaten işlenmiş." });

            returnRequest.Status = ReturnStatus.Approved;
            returnRequest.ProcessedAt = DateTime.UtcNow;
            returnRequest.ProcessedBy = _um.GetUserId(User);
            returnRequest.AdminNotes = adminNotes;

            _db.SaveChanges();
            return Json(new { success = true, message = "İade talebi onaylandı." });
        }

        // Reddet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(int id, string adminNotes)
        {
            var returnRequest = _db.ReturnRequests.FirstOrDefault(r => r.ReturnRequestId == id);
            if (returnRequest == null)
                return Json(new { success = false, message = "İade talebi bulunamadı." });

            if (returnRequest.Status != ReturnStatus.Pending)
                return Json(new { success = false, message = "Bu talep zaten işlenmiş." });

            if (string.IsNullOrWhiteSpace(adminNotes))
                return Json(new { success = false, message = "Lütfen red gerekçesini yazın." });

            returnRequest.Status = ReturnStatus.Rejected;
            returnRequest.ProcessedAt = DateTime.UtcNow;
            returnRequest.ProcessedBy = _um.GetUserId(User);
            returnRequest.AdminNotes = adminNotes;

            _db.SaveChanges();
            return Json(new { success = true, message = "İade talebi reddedildi." });
        }

        // Tamamla
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Complete(int id)
        {
            var returnRequest = _db.ReturnRequests.FirstOrDefault(r => r.ReturnRequestId == id);
            if (returnRequest == null)
                return Json(new { success = false, message = "İade talebi bulunamadı." });

            if (returnRequest.Status != ReturnStatus.Approved)
                return Json(new { success = false, message = "Sadece onaylı talepler tamamlanabilir." });

            returnRequest.Status = ReturnStatus.Completed;
            returnRequest.ProcessedAt ??= DateTime.UtcNow;
            returnRequest.ProcessedBy ??= _um.GetUserId(User);

            _db.SaveChanges();
            return Json(new { success = true, message = "İade işlemi tamamlandı." });
        }

        // Sil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var returnRequest = _db.ReturnRequests
                .Include(r => r.Lines)
                .FirstOrDefault(r => r.ReturnRequestId == id);

            if (returnRequest == null)
                return Json(new { success = false, message = "İade talebi bulunamadı." });

            _db.ReturnRequestLines.RemoveRange(returnRequest.Lines);
            _db.ReturnRequests.Remove(returnRequest);
            _db.SaveChanges();

            return Json(new { success = true, message = "İade talebi silindi." });
        }
    }

    // Liste ekranı için basit VM
    public class AdminReturnRequestListVm
    {
        public List<ReturnRequest> Items { get; set; } = new();
        public string? Status { get; set; }
        public int? OrderId { get; set; }
        public string? Email { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
