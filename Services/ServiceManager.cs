using Services.Contracts;
using StoreApp.Services;

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
        private readonly IProductImageService _productImageService;
        private readonly IProductStockService _productStockService;
        private readonly IEmailService _emailService;
        private readonly IProductQaService _productQaService;



        public ServiceManager(IProductService productService, ICategoryService categoryService, IOrderService orderService, IAuthService authService, IProfileService profileService, IFavoriteService favoriteService, IAddressService addressService, IProductImageService productImageService, IProductStockService productStockService, IEmailService emailService, IProductQaService productQaService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _orderService = orderService;
            _authService = authService;
            _profileService = profileService;
            _favoriteService = favoriteService;
            _addressService = addressService;
            _productImageService = productImageService;
            _productStockService = productStockService;
            _emailService = emailService;
            _productQaService = productQaService;
        }

        public IProductService PoductService => _productService;

        public ICategoryService CategoryService => _categoryService;

        public IOrderService OrderService => _orderService;

        public IAuthService AuthService => _authService;

        public IProfileService ProfileService => _profileService;

        public IAddressService AddressService => _addressService;

        public IFavoriteService FavoriteService => _favoriteService;

        public IProductImageService ProductImageService => _productImageService;

        public IProductStockService ProductStockService => _productStockService;

        public IEmailService EmailService => _emailService;

        public IProductQaService ProductQaService => _productQaService;
    }
}