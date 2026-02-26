using System.Runtime.InteropServices;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Contracts;

namespace StoreApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CouponController : Controller
    {
        private readonly IServiceManager _manager;

        public CouponController(IServiceManager manager)
        {
            _manager = manager;
        }

        private static TimeZoneInfo GetTurkeyTz()
        {
            // Windows: "Turkey Standard Time"
            // Linux: "Europe/Istanbul"
            var tzId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "Turkey Standard Time"
                : "Europe/Istanbul";

            return TimeZoneInfo.FindSystemTimeZoneById(tzId);
        }

        private static DateTime? ToUtcFromTurkey(DateTime? dt)
        {
            if (!dt.HasValue) return null;

            // HTML datetime-local genelde Kind=Unspecified gelir -> TR saati kabul ediyoruz
            var unspecified = DateTime.SpecifyKind(dt.Value, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(unspecified, GetTurkeyTz());
        }
        // LIST
        public async Task<IActionResult> Index(string? q, bool? active)
        {
            var list = await _manager.CouponService.GetAllAsync(q, active);
            return View(list);
        }

        // CREATE GET
        [HttpGet]
        public IActionResult Create()
        {
            var model = new Coupon
            {
                IsActive = true,
                StartsAtUtc = DateTime.UtcNow,
                EndsAtUtc = DateTime.UtcNow.AddDays(7)
            };
            return View(model);
        }

        // CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coupon model)
        {

            // DateTimeKind fix (PostgreSQL timestamptz -> UTC ister)
            if (model.StartsAtUtc.HasValue)
            {
                var v = model.StartsAtUtc.Value;
                model.StartsAtUtc = DateTime.SpecifyKind(v, DateTimeKind.Utc);
            }
            if (model.EndsAtUtc.HasValue)
            {
                var v = model.EndsAtUtc.Value;
                model.EndsAtUtc = DateTime.SpecifyKind(v, DateTimeKind.Utc);
            }
            model.StartsAtUtc = ToUtcFromTurkey(model.StartsAtUtc);
            model.EndsAtUtc = ToUtcFromTurkey(model.EndsAtUtc);
            var (ok, error) = await _manager.CouponService.CreateAsync(model);

            if (!ok)
            {
                // Service tek hata döndürüyor; bunu genel hata olarak basıyoruz
                ModelState.AddModelError(string.Empty, error ?? "Kupon oluşturulamadı.");
                return View(model);
            }

            TempData["Ok"] = "Kupon oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }

        // EDIT GET
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var coupon = await _manager.CouponService.GetByIdAsync(id, trackChanges: false);
            if (coupon == null) return NotFound();
            return View(coupon);
        }

        // EDIT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Coupon model)
        {
            if (model.StartsAtUtc.HasValue)
                model.StartsAtUtc = DateTime.SpecifyKind(model.StartsAtUtc.Value, DateTimeKind.Utc);

            if (model.EndsAtUtc.HasValue)
                model.EndsAtUtc = DateTime.SpecifyKind(model.EndsAtUtc.Value, DateTimeKind.Utc);

            model.StartsAtUtc = ToUtcFromTurkey(model.StartsAtUtc);
            model.EndsAtUtc = ToUtcFromTurkey(model.EndsAtUtc);
            var (ok, error) = await _manager.CouponService.UpdateAsync(id, model);

            if (!ok)
            {
                ModelState.AddModelError(string.Empty, error ?? "Kupon güncellenemedi.");
                return View(model);
            }

            TempData["Ok"] = "Kupon güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        // TOGGLE ACTIVE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            var ok = await _manager.CouponService.ToggleAsync(id);
            if (!ok) return NotFound();

            TempData["Ok"] = "Durum güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        // DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _manager.CouponService.DeleteAsync(id);
            if (!ok) return NotFound();

            TempData["Ok"] = "Kupon silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}