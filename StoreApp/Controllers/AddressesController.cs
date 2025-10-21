using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Entities.Models;
using Repositories;

namespace StoreApp.Controllers
{
    [Authorize]
    public class AddressesController : Controller
    {
        private readonly UserManager<IdentityUser> _um;
        private readonly RepositoryContext _db;

        public AddressesController(UserManager<IdentityUser> um, RepositoryContext db)
        {
            _um = um;
            _db = db;
        }

        public IActionResult Index()
        {
            var userId = _um.GetUserId(User)!;
            var list = _db.UserAddresses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.CreatedAt)
                .ToList();
            return View(list);
        }

        [HttpGet]
        public IActionResult Create() => View(new UserAddress());

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Create(UserAddress dto)
        {
            var userId = _um.GetUserId(User)!;

            // ✅ Kritik: UserId ve User validation'dan çıkar
            ModelState.Remove(nameof(UserAddress.UserId));
            ModelState.Remove(nameof(UserAddress.User));

            if (!ModelState.IsValid)
                return View(dto);

            dto.UserId = userId;
            dto.CreatedAt = DateTime.UtcNow;

            if (dto.IsDefault)
            {
                var my = _db.UserAddresses.Where(a => a.UserId == userId);
                foreach (var a in my)
                    a.IsDefault = false;
            }

            _db.UserAddresses.Add(dto);
            _db.SaveChanges();

            TempData["Success"] = "Adres başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var userId = _um.GetUserId(User)!;
            var addr = _db.UserAddresses
                .FirstOrDefault(a => a.Id == id && a.UserId == userId);

            if (addr == null)
                return NotFound();

            return View(addr);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(UserAddress dto)
        {
            var userId = _um.GetUserId(User)!;

            // ✅ Kritik: UserId ve User validation'dan çıkar
            ModelState.Remove(nameof(UserAddress.UserId));
            ModelState.Remove(nameof(UserAddress.User));

            if (!ModelState.IsValid)
                return View(dto);

            var addr = _db.UserAddresses
                .FirstOrDefault(a => a.Id == dto.Id && a.UserId == userId);

            if (addr == null)
                return NotFound();

            if (dto.IsDefault)
            {
                var my = _db.UserAddresses
                    .Where(a => a.UserId == userId && a.Id != dto.Id);
                foreach (var a in my)
                    a.IsDefault = false;
            }

            addr.FirstName = dto.FirstName;
            addr.LastName = dto.LastName;
            addr.Label = dto.Label;
            addr.City = dto.City;
            addr.District = dto.District;
            addr.Neighborhood = dto.Neighborhood;
            addr.AddressLine = dto.AddressLine;
            addr.PhoneNumber = dto.PhoneNumber;
            addr.IsDefault = dto.IsDefault;

            _db.SaveChanges();

            TempData["Success"] = "Adres başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var userId = _um.GetUserId(User)!;
            var addr = _db.UserAddresses
                .FirstOrDefault(a => a.Id == id && a.UserId == userId);

            if (addr != null)
            {
                _db.UserAddresses.Remove(addr);
                _db.SaveChanges();
                TempData["Success"] = "Adres başarıyla silindi.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult MakeDefault(int id)
        {
            var userId = _um.GetUserId(User)!;
            var addr = _db.UserAddresses
                .FirstOrDefault(a => a.Id == id && a.UserId == userId);

            if (addr == null)
                return RedirectToAction(nameof(Index));

            var my = _db.UserAddresses.Where(a => a.UserId == userId);
            foreach (var a in my)
                a.IsDefault = false;

            addr.IsDefault = true;
            _db.SaveChanges();

            TempData["Success"] = "Varsayılan adres güncellendi.";
            return RedirectToAction(nameof(Index));
        }
    }
}