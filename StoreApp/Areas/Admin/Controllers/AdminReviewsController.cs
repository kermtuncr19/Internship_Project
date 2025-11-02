using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories;

namespace StoreApp.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminReviewsController : Controller
    {
        private readonly RepositoryContext _db;
        public AdminReviewsController(RepositoryContext db)
        {
            _db = db;
        }

        // Tüm yorumları listele (onaylı ve bekleyen)
        public async Task<IActionResult> Index()
        {
            var reviews = await _db.Reviews
                .Include(r => r.Product)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reviews);
        }

        // Yalnızca onay bekleyenler
        public async Task<IActionResult> Pending()
        {
            var pending = await _db.Reviews
                .Include(r => r.Product)
                .Include(r => r.User)
                .Where(r => !r.IsApproved)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View("Index", pending);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var review = await _db.Reviews.FindAsync(id);
            if (review == null)
                return NotFound();

            review.IsApproved = true;
            await _db.SaveChangesAsync();

            TempData["success"] = "Yorum onaylandı.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var review = await _db.Reviews.FindAsync(id);
            if (review == null)
                return NotFound();

            _db.Reviews.Remove(review);
            await _db.SaveChangesAsync();

            TempData["success"] = "Yorum silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
