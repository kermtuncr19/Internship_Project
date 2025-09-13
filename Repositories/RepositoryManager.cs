using System.Xml;
using Repositories.Contracts;
//tüm repolara erişmek için bu repoyu kullanıyoruz.
namespace Repositories
{
    public class RepositoryManager : IRepositoryManager
    {
        private readonly RepositoryContext _context;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryReposiitory;

        public RepositoryManager(IProductRepository productRepository, RepositoryContext context, ICategoryRepository categoryReposiitory)
        {
            _productRepository = productRepository;
            _context = context;
            _categoryReposiitory = categoryReposiitory;
        }

        public IProductRepository Product => _productRepository;

        public ICategoryRepository Category => _categoryReposiitory;
        public void Save()
        {
            _context.SaveChanges();
        }
    }
}