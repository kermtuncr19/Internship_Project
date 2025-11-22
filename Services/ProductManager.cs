using System.Data;
using System.Xml;
using AutoMapper;
using Entities.Dto;
using Entities.Models;
using Entities.RequestParameters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Repositories.Contracts;
using Services.Contracts;

namespace Services
{
    public class ProductManager : IProductService
    {
        private readonly IRepositoryManager _manager;
        private readonly IMapper _mapper;

        public ProductManager(IRepositoryManager manager, IMapper mapper)
        {
            _manager = manager;
            _mapper = mapper;
        }

        public Product CreateProduct(ProductDtoForInsertion productDto)
        {
            Product product = _mapper.Map<Product>(productDto); // mapledik
            _manager.Product.Create(product);
            _manager.Save();
            return product;
        }

        public void DeleteOneProduct(int id)
        {
            Product product = GetOneProduct(id, false);
            if (product is not null)
            {
                _manager.Product.DeleteOneProduct(product);
                _manager.Save();
            }
        }

        public IQueryable<Product> GetAllProducts(bool trackChanges)
        {
            return _manager.Product.GetAllProducts(trackChanges);
        }

        public IEnumerable<Product> GetAllProductsWithDetails(ProductRequestParameters p)
        {
            return _manager.Product.GetAllProductsWithDetails(p);
        }

        public Product? GetOneProduct(int id, bool trackChanges)
        {
            var product = _manager.Product.GetOneProduct(id, trackChanges);
            if (product is null)
                throw new Exception("Product not found.");
            return product;
        }

        public ProductDtoForUpdate GetOneProductForUpdate(int id, bool trackChanges)
        {
            var product = GetOneProduct(id, trackChanges);
            var productDto = _mapper.Map<ProductDtoForUpdate>(product);
            return productDto;
        }

        public IEnumerable<Product> GetShowcaseProducts(bool trackChanges)
        {
            var products = _manager.Product.GetShowcaseProducts(trackChanges);
            return products;
        }

        public void UpdateOneProduct(ProductDtoForUpdate productDto)
        {
            // 1) Mevcut ürünü tracked olarak çek
            var entity = _manager.Product.GetOneProduct(productDto.ProductId, true);
            if (entity is null) throw new Exception("Product not found.");

            // 2) Eğer dosya yüklenmemişse, mevcut ImageUrl'i koru (controller tarafında da yapabilirsiniz)
            if (string.IsNullOrWhiteSpace(productDto.ImageUrl))
                productDto.ImageUrl = entity.ImageUrl;

            // 3) DTO -> entity (mevcut nesnenin üzerine)
            _mapper.Map(productDto, entity);

            // 4) Sadece Save
            _manager.Save();
        }

        public async Task<IEnumerable<ProductSalesViewModel>> GetTopSellingProductsAsync(int count)
        {
            var monthAgo = DateTime.UtcNow.AddMonths(-1);

            // Order'ları çek ve grupla
            var topProducts = await _manager.Order.Orders
                .Where(o => o.OrderedAt >= monthAgo && o.Shipped && !o.Cancelled)
                .SelectMany(o => o.Lines)
                .GroupBy(l => l.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    SalesCount = g.Sum(l => l.Quantity),
                    TotalRevenue = g.Sum(l => l.UnitPrice * l.Quantity)
                })
                .OrderByDescending(p => p.TotalRevenue)
                .Take(count)
                .ToListAsync();

            var result = new List<ProductSalesViewModel>();

            foreach (var item in topProducts)
            {
                var product = _manager.Product.GetOneProduct(item.ProductId, false);

                if (product != null)
                {
                    result.Add(new ProductSalesViewModel
                    {
                        ProductId = item.ProductId,
                        Name = product.ProductName ?? "Bilinmeyen Ürün",
                        ImageUrl = product.ImageUrl ?? "/images/no-image.png",
                        SalesCount = item.SalesCount,
                        TotalRevenue = item.TotalRevenue
                    });
                }
            }

            return result;
        }


    }
}