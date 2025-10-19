using System.Drawing;
using Entities.Dto;
using Entities.Models;
using StoreApp.Models;
using System.Linq;
using Entities.RequestParameters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Services.Contracts;
using Repositories.Extensions;

namespace StoreApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly IServiceManager _manager;

        public ProductController(IServiceManager manager)
        {
            _manager = manager;
        }

        public IActionResult Index([FromQuery] ProductRequestParameters p)
        {
            ViewData["Title"] = "Ürünler";
            // Fiyat aralığı koruması (opsiyonel ama önerilir)
            if (!p.IsValidPrice)
            {
                TempData["PriceError"] = "Maksimum fiyat, minimum fiyattan küçük olamaz.";
                p.MinPrice = 0;
                p.MaxPrice = int.MaxValue;
            }

            // Kategori pill'leri için gerekli ViewBag'ler
            ViewBag.Categories = _manager.CategoryService.GetAllCategories(trackChanges: false);
            ViewBag.ActiveCategoryId = p.CategoryId;

            // 1) Filtreleri UYGULA (henüz Skip/Take yok)
            var q = _manager.PoductService.GetAllProducts(false)
                .FilteredByCategoryId(p.CategoryId)
                .FilteredBySearchTerm(p.SearchTerm)
                .FilteredByPrice(p.MinPrice, p.MaxPrice, p.IsValidPrice)
                .OrderBy(x => x.ProductId); // deterministik sıralama

            // 2) Toplam (filtreli)
            var total = q.Count();

            // 3) Sayfalık veri
            var items = q.Skip((p.PageNumber - 1) * p.PageSize)
                         .Take(p.PageSize)
                         .ToList();

            var pagination = new Pagination
            {
                CurrentPage = p.PageNumber < 1 ? 1 : p.PageNumber,
                ItemsPerPage = p.PageSize < 1 ? 12 : p.PageSize,
                TotalItems = total
            };

            var vm = new ProductListViewModel
            {
                Products = items,
                Pagination = pagination
            };

            return View(vm); // ✅ Artık ViewModel gidiyor
        }


        public IActionResult Create()
        {
            ViewBag.Categories = GetCategoriesSelectList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] ProductDtoForInsertion productDto, IFormFile? file)
        {
            // ✅ ModelState hatalarını yakala ve logla
            if (!ModelState.IsValid)
            {
                var errs = ModelState
                    .Where(kv => kv.Value?.Errors.Count > 0)
                    .Select(kv => new { Field = kv.Key, Errors = kv.Value!.Errors.Select(e => e.ErrorMessage) });

                // İstersen debug için konsola yazdır:
                foreach (var err in errs)
                {
                    Console.WriteLine($"Alan: {err.Field}, Hatalar: {string.Join(", ", err.Errors)}");
                }

                // View tekrar çizilirken kategori dropdown'ı kaybolmasın
                ViewBag.Categories = GetCategoriesSelectList();
                return View(productDto);
            }

            // ✅ ModelState geçerliyse dosyayı kaydet ve yönlendir
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("file", "Lütfen bir ürün görseli yükleyin.");
                ViewBag.Categories = GetCategoriesSelectList();
                return View(productDto);
            }

            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", file.FileName);
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            productDto.ImageUrl = $"/images/{file.FileName}";
            _manager.PoductService.CreateProduct(productDto);
            TempData["success"] = $"{productDto.ProductName} isimli ürün başarıyla oluşturuldu.";

            return RedirectToAction(nameof(Index), new { area = "Admin" });
        }



        private SelectList GetCategoriesSelectList()
        {
            return new SelectList(_manager.CategoryService.GetAllCategories(false), "CategoryId", "CategoryName", "1");
        }


        public IActionResult Update([FromRoute(Name = "id")] int id)
        {
            ViewBag.Categories = GetCategoriesSelectList();
            var model = _manager.PoductService.GetOneProductForUpdate(id, false);
            ViewData["Title"] = model?.ProductName;
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([FromForm] ProductDtoForUpdate productDto, IFormFile? file)
        {
            // GEÇİCİ LOG (form binding kontrolü)
            System.Diagnostics.Debug.WriteLine($"DTO.RequiresSize={productDto.RequiresSize}, DTO.SizeOptionsCsv='{productDto.SizeOptionsCsv}'");

            if (ModelState.IsValid)
            {
                // ✅ Sadece yeni dosya yüklenmişse fotoğrafı güncelle
                if (file != null && file.Length > 0)
                {
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", file.FileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    productDto.ImageUrl = String.Concat("/images/", file.FileName);
                }
                // ✅ Eğer file null ise, hidden input'tan gelen ImageUrl zaten productDto içinde

                _manager.PoductService.UpdateOneProduct(productDto);
                TempData["success"] = "Ürün başarıyla güncellendi.";
                return RedirectToAction("Index");
            }

            // ✅ Hata varsa kategorileri tekrar yükle
            ViewBag.Categories = GetCategoriesSelectList();
            return View(productDto);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _manager.PoductService.DeleteOneProduct(id);
            TempData["Danger"] = "Ürün silindi.";
            return RedirectToAction(nameof(Index));
        }


    }
}