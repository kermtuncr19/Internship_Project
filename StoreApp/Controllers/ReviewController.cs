using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories;
using StoreApp.Models;

namespace StoreApp.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly RepositoryContext _db;
        private readonly UserManager<IdentityUser> _um;

        public ReviewController(RepositoryContext db, UserManager<IdentityUser> um)
        {
            _db = db;
            _um = um;
        }

        [HttpGet]
        public IActionResult Create(int productId, int orderId)
        {
            var userId = _um.GetUserId(User);
            
            // Siparişin kullanıcıya ait olduğunu ve teslim edildiğini kontrol et
            var order = _db.Orders
                .Include(o => o.Lines)
                .ThenInclude(l => l.Product)
                .FirstOrDefault(o => o.OrderId == orderId && o.UserId == userId && o.Delivered);
            
            if (order == null)
            {
                TempData["error"] = "Sadece teslim edilen ürünlere yorum yapabilirsiniz.";
                return RedirectToAction("Index", "MyOrders");
            }
            
            // Ürünün sipariş içinde olduğunu kontrol et
            var orderLine = order.Lines.FirstOrDefault(l => l.ProductId == productId);
            if (orderLine == null)
            {
                TempData["error"] = "Bu ürün siparişinizde bulunmuyor.";
                return RedirectToAction("Detail", "MyOrders", new { id = orderId });
            }
            
            // Daha önce yorum yapılmış mı kontrol et
            var existingReview = _db.Reviews.FirstOrDefault(r => r.ProductId == productId && r.OrderId == orderId && r.UserId == userId);
            
            var viewModel = new ReviewViewModel
            {
                ProductId = productId,
                ProductName = orderLine.Product?.ProductName,
                ProductImageUrl = orderLine.Product?.ImageUrl,
                OrderId = orderId,
                Rating = existingReview?.Rating ?? 0,
                Comment = existingReview?.Comment,
                ShowFullName = existingReview?.ShowFullName ?? false
            };
            
            ViewBag.IsEdit = existingReview != null;
            
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ReviewViewModel model)
        {
            var userId = _um.GetUserId(User);
            
            // Product bilgisini yeniden al
            var product = _db.Products.Find(model.ProductId);
            if (product != null)
            {
                model.ProductName = product.ProductName;
                model.ProductImageUrl = product.ImageUrl;
            }
            
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            // Siparişin kullanıcıya ait olduğunu ve teslim edildiğini tekrar kontrol et
            var order = _db.Orders
                .Include(o => o.Lines)
                .FirstOrDefault(o => o.OrderId == model.OrderId && o.UserId == userId && o.Delivered);
            
            if (order == null)
            {
                TempData["error"] = "Sadece teslim edilen ürünlere yorum yapabilirsiniz.";
                return RedirectToAction("Index", "MyOrders");
            }
            
            // Ürünün sipariş içinde olduğunu kontrol et
            if (!order.Lines.Any(l => l.ProductId == model.ProductId))
            {
                TempData["error"] = "Bu ürün siparişinizde bulunmuyor.";
                return RedirectToAction("Detail", "MyOrders", new { id = model.OrderId });
            }
            
            // Mevcut yorumu kontrol et
            var existingReview = _db.Reviews.FirstOrDefault(r => r.ProductId == model.ProductId && r.OrderId == model.OrderId && r.UserId == userId);
            
            if (existingReview != null)
            {
                // Güncelleme
                existingReview.Rating = model.Rating;
                existingReview.Comment = model.Comment;
                existingReview.CreatedAt = DateTime.UtcNow;
                existingReview.IsApproved = false;
                
                TempData["success"] = "Yorumunuz güncellendi ve onay bekliyor.";
            }
            else
            {
                // Yeni yorum
                var review = new Review
                {
                    ProductId = model.ProductId,
                    UserId = userId,
                    OrderId = model.OrderId,
                    Rating = model.Rating,
                    Comment = model.Comment,
                    CreatedAt = DateTime.UtcNow,
                    IsApproved = false
                };
                
                _db.Reviews.Add(review);
                TempData["success"] = "Yorumunuz alındı ve onay bekliyor.";
            }
            
            _db.SaveChanges();
            
            return RedirectToAction("Detail", "MyOrders", new { id = model.OrderId });
        }
    }
}