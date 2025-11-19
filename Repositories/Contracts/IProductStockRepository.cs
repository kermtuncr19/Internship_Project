using Entities.Models;

namespace Repositories.Contracts
{
    public interface IProductStockRepository : IRepositoryBase<ProductStock>
    {
        IQueryable<ProductStock> GetStocksByProductId(int productId, bool trackChanges);
        ProductStock? GetStockByProductAndSize(int productId, string? size, bool trackChanges);
        void CreateStock(ProductStock stock);
        void UpdateStock(ProductStock stock);
        void DeleteStock(ProductStock stock);
    }
}