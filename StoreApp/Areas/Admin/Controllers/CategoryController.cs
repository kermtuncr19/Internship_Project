using Entities.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Contracts;

namespace StoreApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly IServiceManager _manager;
        public CategoryController(IServiceManager manager) => _manager = manager;

        public IActionResult Index() =>
            View(_manager.CategoryService.GetAllCategories(false));

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Create(CategoryForCreateDto dto)
        {
            if (_manager.CategoryService.ExistsByName(dto.CategoryName))
                ModelState.AddModelError(nameof(dto.CategoryName), "Bu kategori adı zaten mevcut.");

            if (!ModelState.IsValid) return View(dto);

            _manager.CategoryService.CreateCategory(dto);
            
            return RedirectToAction(nameof(Index));
        }

        // ---- EDIT ----
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var entity = _manager.CategoryService.GetOneCategory(id, false);
            if (entity is null) return NotFound();

            var vm = new CategoryForUpdateDto
            {
                CategoryId = entity.CategoryId,
                CategoryName = entity.CategoryName
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(CategoryForUpdateDto dto)
        {
            // Aynı ad başka bir kategoride var mı?
            if (_manager.CategoryService.ExistsByName(dto.CategoryName, excludeId: dto.CategoryId))
                ModelState.AddModelError(nameof(dto.CategoryName), "Bu kategori adı zaten mevcut.");

            if (!ModelState.IsValid) return View(dto);

            try
            {
                _manager.CategoryService.UpdateCategory(dto);
                
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                // ExistsByName kontrolüne rağmen bir kenar durum
                ModelState.AddModelError(nameof(dto.CategoryName), ex.Message);
                return View(dto);
            }
        }
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var entity = _manager.CategoryService.GetOneCategory(id, false);
            if (entity is null)
            {
                TempData["error"] = "Kategori bulunamadı.";
                return RedirectToAction(nameof(Index));
            }
            return View(entity); // onay sayfası
        }

        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            if (_manager.CategoryService.TryDeleteCategory(id, false, out var error))
            {
                
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = error ?? "Kategori silinemedi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
