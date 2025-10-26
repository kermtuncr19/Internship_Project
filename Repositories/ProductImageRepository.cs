using Entities.Models;
using Repositories.Contracts;

namespace Repositories
{
    public class ProductImageRepository : RepositoryBase<ProductImage>, IProductImageRepository
    {
        public ProductImageRepository(RepositoryContext context) : base(context)
        {
        }
    }
}