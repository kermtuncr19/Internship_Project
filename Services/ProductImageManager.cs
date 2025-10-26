// ProductImageManager.cs (Services klasörü)
using Entities.Models;
using Repositories.Contracts;
using Services.Contracts;
using System.Linq;
using System.Collections.Generic;

namespace Services
{
    public class ProductImageManager : IProductImageService
    {
        private readonly IRepositoryManager _manager;

        public ProductImageManager(IRepositoryManager manager)
        {
            _manager = manager;
        }

        public IEnumerable<ProductImage> GetAllProductImages(bool trackChanges)
        {
            return _manager.ProductImage.FindAll(trackChanges).ToList();
        }

        public ProductImage? GetOneProductImage(int id, bool trackChanges)
        {
            return _manager.ProductImage
                .FindByCondition(p => p.ProductImageId == id, trackChanges);
        }

        public void CreateProductImage(ProductImage productImage)
        {
            _manager.ProductImage.Create(productImage);
            _manager.Save();
        }

        public void UpdateProductImage(ProductImage productImage)
        {
            _manager.ProductImage.Update(productImage);
            _manager.Save();
        }

        public void UpdateProductImages(List<ProductImage> productImages)
        {
            foreach (var image in productImages)
            {
                _manager.ProductImage.Update(image);
            }
            _manager.Save();
        }

        public void DeleteProductImage(int id)
        {
            var image = GetOneProductImage(id, false);
            if (image != null)
            {
                // ✅ Delete yerine Remove deneyin (eğer IRepositoryBase'de Remove varsa)
                _manager.ProductImage.Remove(image);
                _manager.Save();
            }
        }
    }
}