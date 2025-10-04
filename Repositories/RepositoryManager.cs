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
        private readonly IOrderRepository _orderRepository;

        public RepositoryManager(IProductRepository productRepository, RepositoryContext context, ICategoryRepository categoryReposiitory, IOrderRepository orderRepository)
        {
            _productRepository = productRepository;
            _context = context;
            _categoryReposiitory = categoryReposiitory;
            _orderRepository = orderRepository;
        }

        public IProductRepository Product => _productRepository;

        public ICategoryRepository Category => _categoryReposiitory;

        public IOrderRepository Order => _orderRepository;

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}