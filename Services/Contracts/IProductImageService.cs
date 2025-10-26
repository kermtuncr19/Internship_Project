using Entities.Models;

namespace Services.Contracts
{
    public interface IProductImageService
    {
        IEnumerable<ProductImage> GetAllProductImages(bool trackChanges);
        ProductImage? GetOneProductImage(int id, bool trackChanges);
        void CreateProductImage(ProductImage productImage);
        void UpdateProductImage(ProductImage productImage);
        void UpdateProductImages(List<ProductImage> productImages);
        void DeleteProductImage(int id);
    }
}