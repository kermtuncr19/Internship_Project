using Entities.Models;
using Repositories.Contracts;

namespace Repositories
{
    public class ProductStockRepository : RepositoryBase<ProductStock>, IProductStockRepository
    {
        public ProductStockRepository(RepositoryContext context) : base(context)
        {
        }

        public IQueryable<ProductStock> GetStocksByProductId(int productId, bool trackChanges)
        {
            return FindAll(trackChanges)
                .Where(s => s.ProductId == productId)
                .OrderBy(s => s.Size);
        }

        public ProductStock? GetStockByProductAndSize(int productId, string? size, bool trackChanges)
        {
            return FindByCondition(s => s.ProductId == productId && s.Size == size, trackChanges);
        }

        public void CreateStock(ProductStock stock) => Create(stock);

        public void UpdateStock(ProductStock stock) => Update(stock);

        public void DeleteStock(ProductStock stock) => Remove(stock);
    }
}