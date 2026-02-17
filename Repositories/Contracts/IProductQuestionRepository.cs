using Entities.Models;

namespace Repositories.Contracts
{
    public interface IProductQuestionRepository
    {
        IQueryable<ProductQuestion> Query(bool trackChanges);

        void Create(ProductQuestion q);
        ProductQuestion? GetOne(int id, bool trackChanges);
    }
}
