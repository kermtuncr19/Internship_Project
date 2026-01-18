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
using Microsoft.EntityFrameworkCore;

namespace StoreApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly IServiceManager _manager;
        private readonly IStorageService _storage;

        public ProductController(IServiceManager manager, IStorageService storage)
        {
            _manager = manager;
            _storage = storage;
        }

        public IActionResult Index([FromQuery] ProductRequestParameters p)
        {
            ViewData["Title"] = "Ürünler";

            if (!p.IsValidPrice)
            {
                TempData["PriceError"] = "Maksimum fiyat, minimum fiyattan küçük olamaz.";
                p.MinPrice = 0;
                p.MaxPrice = int.MaxValue;
            }

            ViewBag.Categories = _manager.CategoryService.GetAllCategories(trackChanges: false);
            ViewBag.ActiveCategoryId = p.CategoryId;

            var q = _manager.PoductService.GetAllProducts(false)
                .Include(p => p.Stocks)
                .FilteredByCategoryId(p.CategoryId)
                .FilteredBySearchTerm(p.SearchTerm)
                .FilteredByPrice(p.MinPrice, p.MaxPrice, p.IsValidPrice)
                .OrderBy(x => x.ProductId);

            var total = q.Count();

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

            return View(vm);
        }

        public IActionResult Create()
        {
            ViewBag.Categories = GetCategoriesSelectList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [FromForm] ProductDtoForInsertion productDto,
            IFormFile? file,
            IFormFileCollection? additionalImages)
        {
            if (!ModelState.IsValid)
            {
                var errs = ModelState
                    .Where(kv => kv.Value?.Errors.Count > 0)
                    .Select(kv => new { Field = kv.Key, Errors = kv.Value!.Errors.Select(e => e.ErrorMessage) });

                foreach (var err in errs)
                {
                    Console.WriteLine($"Alan: {err.Field}, Hatalar: {string.Join(", ", err.Errors)}");
                }

                ViewBag.Categories = GetCategoriesSelectList();
                return View(productDto);
            }

            // ✅ 1) Ana foto kontrolü
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("file", "Lütfen bir ürün görseli yükleyin.");
                ViewBag.Categories = GetCategoriesSelectList();
                return View(productDto);
            }

            // ✅ 2) Ana fotoğrafı bucket'a yükle
            var (_, mainPublicUrl) = await _storage.UploadAsync(file, "images");
            productDto.ImageUrl = mainPublicUrl;

            // ✅ 3) Ürünü oluştur
            var product = _manager.PoductService.CreateProduct(productDto);

            // ✅ 4) Ana fotoğrafı ProductImages tablosuna ekle
            var mainImage = new ProductImage
            {
                ProductId = product.ProductId,
                ImageUrl = mainPublicUrl,
                DisplayOrder = 0,
                IsMain = true
            };
            _manager.ProductImageService.CreateProductImage(mainImage);

            // ✅ 5) Ek fotoğrafları ekle
            if (additionalImages != null && additionalImages.Count > 0)
            {
                int order = 1;

                foreach (var additionalImage in additionalImages)
                {
                    if (additionalImage.Length > 0)
                    {
                        var (_, publicUrl) = await _storage.UploadAsync(additionalImage, "images");

                        var productImage = new ProductImage
                        {
                            ProductId = product.ProductId,
                            ImageUrl = publicUrl,
                            DisplayOrder = order++,
                            IsMain = false
                        };

                        _manager.ProductImageService.CreateProductImage(productImage);
                    }
                }
            }

            TempData["success"] = $"{productDto.ProductName} isimli ürün başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index), new { area = "Admin" });
        }

        public IActionResult Update([FromRoute(Name = "id")] int id)
        {
            ViewBag.Categories = GetCategoriesSelectList();
            var model = _manager.PoductService.GetOneProductForUpdate(id, false);
            ViewData["Title"] = model?.ProductName;

            // ✅ Ürünün fotoğraflarını ViewBag'e ekle
            ViewBag.ProductImages = _manager.PoductService
                .GetAllProducts(false)
                .Include(p => p.Images)
                .FirstOrDefault(p => p.ProductId == id)?
                .Images
                .OrderBy(i => i.DisplayOrder)
                .Select(i => new {
                    i.ProductImageId,
                    i.ImageUrl,
                    i.DisplayOrder,
                    i.IsMain
                })
                .ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            [FromForm] ProductDtoForUpdate productDto,
            IFormFile? file,
            IFormFileCollection? newImages)
        {
            System.Diagnostics.Debug.WriteLine($"DTO.RequiresSize={productDto.RequiresSize}, DTO.SizeOptionsCsv='{productDto.SizeOptionsCsv}'");

            if (ModelState.IsValid)
            {
                // ✅ 1) Ana fotoğraf güncelleme (sadece yeni dosya yüklenmişse)
                if (file != null && file.Length > 0)
                {
                    var (_, publicUrl) = await _storage.UploadAsync(file, "images");
                    productDto.ImageUrl = publicUrl;
                }

                _manager.PoductService.UpdateOneProduct(productDto);

                // ✅ 2) Yeni fotoğraflar ekleme
                if (newImages != null && newImages.Count > 0)
                {
                    var product = _manager.PoductService
                        .GetAllProducts(false)
                        .Include(p => p.Images)
                        .FirstOrDefault(p => p.ProductId == productDto.ProductId);

                    if (product != null)
                    {
                        var currentMaxOrder = product.Images.Any() ? product.Images.Max(i => i.DisplayOrder) : -1;

                        foreach (var newImage in newImages)
                        {
                            if (newImage.Length > 0)
                            {
                                var (_, publicUrl) = await _storage.UploadAsync(newImage, "images");

                                var productImage = new ProductImage
                                {
                                    ProductId = productDto.ProductId,
                                    ImageUrl = publicUrl,
                                    DisplayOrder = ++currentMaxOrder,
                                    IsMain = !product.Images.Any()
                                };

                                _manager.ProductImageService.CreateProductImage(productImage);
                            }
                        }
                    }
                }

                TempData["success"] = "Ürün başarıyla güncellendi.";
                return RedirectToAction("Index");
            }

            ViewBag.Categories = GetCategoriesSelectList();
            return View(productDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetMainImage([FromBody] SetMainImageRequest request)
        {
            try
            {
                var product = _manager.PoductService
                    .GetAllProducts(false)
                    .Include(p => p.Images)
                    .FirstOrDefault(p => p.ProductId == request.ProductId);

                if (product == null) return NotFound();

                foreach (var img in product.Images)
                {
                    img.IsMain = false;
                }

                var selectedImage = product.Images.FirstOrDefault(i => i.ProductImageId == request.ImageId);
                if (selectedImage != null)
                {
                    selectedImage.IsMain = true;
                }

                _manager.ProductImageService.UpdateProductImages(product.Images.ToList());
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ✅ Fotoğraf silme (Bucket + DB)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int id)
        {
            try
            {
                var image = _manager.ProductImageService.GetOneProductImage(id, false);
                if (image == null) return NotFound();

                // ✅ Bucket'tan sil (ImageUrl imgproxy formatındaysa key çıkar)
                if (!string.IsNullOrWhiteSpace(image.ImageUrl))
                {
                    var marker = "/unsafe/plain/s3://";
                    var idx = image.ImageUrl.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
                    if (idx >= 0)
                    {
                        // tail: BUCKET/images/xxx.jpg
                        var tail = image.ImageUrl[(idx + marker.Length)..];
                        var firstSlash = tail.IndexOf('/');
                        if (firstSlash > 0 && firstSlash < tail.Length - 1)
                        {
                            // key: images/xxx.jpg
                            var key = tail[(firstSlash + 1)..];
                            await _storage.DeleteAsync(key);
                        }
                    }
                }

                _manager.ProductImageService.DeleteProductImage(id);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _manager.PoductService.DeleteOneProduct(id);
            TempData["Danger"] = "Ürün silindi.";
            return RedirectToAction(nameof(Index));
        }

        private SelectList GetCategoriesSelectList()
        {
            return new SelectList(_manager.CategoryService.GetAllCategories(false), "CategoryId", "CategoryName", "1");
        }
    }

    public class SetMainImageRequest
    {
        public int ProductId { get; set; }
        public int ImageId { get; set; }
    }
}
