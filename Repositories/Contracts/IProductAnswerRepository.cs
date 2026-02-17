using Entities.Models;

namespace Repositories.Contracts
{
    public interface IProductAnswerRepository
    {
        IQueryable<ProductAnswer> Query(bool trackChanges);

        void Create(ProductAnswer a);
        ProductAnswer? GetByQuestionId(int questionId, bool trackChanges);
    }
}
