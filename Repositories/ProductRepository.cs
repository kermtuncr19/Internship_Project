using Entities.Models;
using Entities.RequestParameters;
using Microsoft.EntityFrameworkCore;
using Repositories.Contracts;
using Repositories.Extensions;

namespace Repositories
{
    public class ProductRepository : RepositoryBase<Product>, IProductRepository
    {
        public ProductRepository(RepositoryContext context) : base(context)
        {
        }

        public void CreateOneProduct(Product product) => Create(product);

        public void DeleteOneProduct(Product product) => Remove(product);


        public IQueryable<Product> GetAllProducts(bool trackChange) => FindAll(trackChange);

        public IQueryable<Product> GetAllProductsWithDetails(ProductRequestParameters p)
        {
            return _context
                .Products
                .FilteredByCategoryId(p.CategoryId)
                .FilteredBySearchTerm(p.SearchTerm);

        }

        //Interface
        public Product? GetOneProduct(int id, bool trackChange)
        {
            return FindByCondition(p => p.ProductId.Equals(id), trackChange);
        }

        public IQueryable<Product> GetShowcaseProducts(bool trackChange)
        {
            return FindAll(trackChange)
                    .Where(p => p.ShowCase.Equals(true));
        }

        public void UpdateOneProduct(Product entity) => Update(entity);
    }
}