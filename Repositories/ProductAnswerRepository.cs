using Entities.Models;
using Repositories.Contracts;

namespace Repositories
{
    public class ProductAnswerRepository : RepositoryBase<ProductAnswer>, IProductAnswerRepository
    {
        public ProductAnswerRepository(RepositoryContext context) : base(context) { }

        public IQueryable<ProductAnswer> Query(bool trackChanges) => FindAll(trackChanges);

        public void Create(ProductAnswer a) => base.Create(a);

        public ProductAnswer? GetByQuestionId(int questionId, bool trackChanges) =>
            FindOne(x => x.ProductQuestionId == questionId, trackChanges);
    }
}
