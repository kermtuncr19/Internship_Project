using StoreApp.Services;

namespace Services.Contracts
{
    public interface IServiceManager
    {
        IProductService PoductService { get; }
        ICategoryService CategoryService { get; }
        IOrderService OrderService { get; }
        IAuthService AuthService { get; }
        IProfileService ProfileService { get; }
        IAddressService AddressService { get; }
        IFavoriteService FavoriteService { get; }
        IProductImageService ProductImageService { get; }
        IProductStockService ProductStockService { get; }
        IEmailService EmailService {get;}
        IProductQaService ProductQaService { get; }

    }
}