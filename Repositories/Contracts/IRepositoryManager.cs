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
        void Save();
    }
}