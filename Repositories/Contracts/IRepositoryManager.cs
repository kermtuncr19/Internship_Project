namespace Repositories.Contracts
{
    public interface IRepositoryManager
    {
        IProductRepository Product { get; }
        ICategoryRepository Category { get; }
        IOrderRepository Order { get; }
        IUserProfileRepository UserProfile { get; }
        IUserAddressRepository UserAddress { get; }
        IFavoriteRepository Favorite { get; }
        IProductImageRepository ProductImage { get; } 
        IReturnRequestRepository ReturnRequest { get; }
        IProductStockRepository ProductStock { get; }
        void Save();
    }
}