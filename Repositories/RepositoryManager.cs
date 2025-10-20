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
        private readonly IUserProfileRepository _userProfile;
        private readonly IUserAddressRepository _userAddress;
        private readonly IFavoriteRepository _favorite;


        public RepositoryManager(IProductRepository productRepository, RepositoryContext context, ICategoryRepository categoryReposiitory, IOrderRepository orderRepository, IUserProfileRepository userProfile,
        IUserAddressRepository userAddress,
        IFavoriteRepository favorite)
        {
            _productRepository = productRepository;
            _context = context;
            _categoryReposiitory = categoryReposiitory;
            _orderRepository = orderRepository;
            _userProfile = userProfile;
            _userAddress = userAddress;
            _favorite = favorite;
        }

        public IProductRepository Product => _productRepository;

        public ICategoryRepository Category => _categoryReposiitory;

        public IOrderRepository Order => _orderRepository;

        public IUserProfileRepository UserProfile => _userProfile;

        public IUserAddressRepository UserAddress => _userAddress;

        public IFavoriteRepository Favorite => _favorite;

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}