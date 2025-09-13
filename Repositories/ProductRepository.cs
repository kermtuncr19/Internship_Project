using Entities.Models;
using Repositories.Contracts;

namespace Repositories
{
    public class ProductRepository : RepositoryBase<Product>, IProductRepository
    {
        public ProductRepository(RepositoryContext context) : base(context)
        {
        }

        public IQueryable<Product> GetAllProducts(bool trackChange) => FindAll(trackChange);

        //Interface
        public Product? GetOneProduct(int id, bool trackChange)
        {
            return FindByCondition(p => p.Id.Equals(id), trackChange);
        }
    }
}