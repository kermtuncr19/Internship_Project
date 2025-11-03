using Entities.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Repositories;
using Repositories.Contracts;
using Services;
using Services.Contracts;
using StoreApp.Models;

namespace StoreApp.Infrastructure.Extensions
{
    public static class ServiceExtension
    {
        public static void ConfigureDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<RepositoryContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("sqlconnection"),
                b => b.MigrationsAssembly("StoreApp"));

                options.EnableSensitiveDataLogging(true);
                // geliştrime aşamasında hassas bilgileri loglara yansıtmak için sonrasında kaldıracağız.
            });//veritabanı için servis kaydı.
        }

        public static void ConfigureIdentity(this IServiceCollection services)
        {
            services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ çğıöşüÇĞİÖŞÜ ";

                options.SignIn.RequireConfirmedAccount = false;
                options.User.RequireUniqueEmail = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 8;
            })
            .AddPasswordValidator<UnicodePasswordValidator<IdentityUser>>()
            .AddEntityFrameworkStores<RepositoryContext>();


        }

        public static void ConfigureSession(this IServiceCollection services)
        {
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.Name = "StoreApp.Session";
                options.IdleTimeout = TimeSpan.FromMinutes(10);
                //10 dk içinde bir istekte bulunulmazsa o oturum düşürülecek.
            });
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<Cart>(c => SessionCart.GetCart(c));//singleton yerine scoped dediğimiz için kullanıcıların sepetleri artık ayrıldı ve sessiondan gelen cartı bize vermesini istiyoruz.
        }
        public static void ConfigureRepositoryRegistration(this IServiceCollection services)
        {
            services.AddScoped<IRepositoryManager, RepositoryManager>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IUserProfileRepository, UserProfileRepository>();
            services.AddScoped<IUserAddressRepository, UserAddressRepository>();
            services.AddScoped<IFavoriteRepository, FavoriteRepository>();
            services.AddScoped<IProductImageRepository, ProductImageRepository>();
            services.AddScoped<IReturnRequestRepository, ReturnRequestRepository>();
        }
        public static void ConfigureServiceRegistration(this IServiceCollection services)
        {
            services.AddScoped<IServiceManager, ServiceManager>();
            services.AddScoped<IProductService, ProductManager>();
            services.AddScoped<ICategoryService, CategoryManager>();
            services.AddScoped<IOrderService,
            OrderManager>();
            services.AddScoped<IAuthService, AuthManager>();
            services.AddScoped<IProfileService, ProfileManager>();
            services.AddScoped<IAddressService, AddressManager>();
            services.AddScoped<IFavoriteService, FavoriteManager>();
            services.AddScoped<IProductImageService, ProductImageManager>();
            services.AddScoped<IReturnRequestService, ReturnRequestManager>();
        }

        public static void ConfigureApplicationCookie(this IServiceCollection services)
        {
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = new PathString("/Account/Login");
                options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
                options.AccessDeniedPath = new PathString("/Account/AccessDenied");
            });
        }
        public static void ConfigureRouting(this IServiceCollection services)
        {
            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
                //options.AppendTrailingSlash = true;
            });
        }
    }
}