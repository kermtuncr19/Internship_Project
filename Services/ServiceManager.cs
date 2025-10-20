using Services.Contracts;

namespace Services
{
    public class ServiceManager : IServiceManager
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IOrderService _orderService;
        private readonly IAuthService _authService;
        private readonly IProfileService _profileService;
        private readonly IFavoriteService _favoriteService;
        private readonly IAddressService _addressService;


        public ServiceManager(IProductService productService, ICategoryService categoryService, IOrderService orderService, IAuthService authService, IProfileService profileService, IFavoriteService favoriteService, IAddressService addressService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _orderService = orderService;
            _authService = authService;
            _profileService = profileService;
            _favoriteService = favoriteService;
            _addressService = addressService;
        }

        public IProductService PoductService => _productService;

        public ICategoryService CategoryService => _categoryService;

        public IOrderService OrderService => _orderService;

        public IAuthService AuthService => _authService;

        public IProfileService ProfileService => _profileService;

        public IAddressService AddressService => _addressService;

        public IFavoriteService FavoriteService => _favoriteService;
    }
}