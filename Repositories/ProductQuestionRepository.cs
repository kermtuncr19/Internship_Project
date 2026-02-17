using Entities.Models;
using Repositories.Contracts;

namespace Repositories
{
    public class ProductQuestionRepository : RepositoryBase<ProductQuestion>, IProductQuestionRepository
    {
        public ProductQuestionRepository(RepositoryContext context) : base(context) { }

        public IQueryable<ProductQuestion> Query(bool trackChanges) => FindAll(trackChanges);

        public void Create(ProductQuestion q) => base.Create(q);

        public ProductQuestion? GetOne(int id, bool trackChanges) =>
            FindOne(x => x.ProductQuestionId == id, trackChanges);
    }
}
