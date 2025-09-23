namespace Services.Contracts
{
    public interface IServiceManager
    {
        IProductService PoductService { get; }
        ICategoryService CategoryService{ get; }
    }
}