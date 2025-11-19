using System.Reflection;
using Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Repositories.Config;

namespace Repositories
{
    public class RepositoryContext : IdentityDbContext<IdentityUser>
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<UserAddress> UserAddresses { get; set; }
        public DbSet<UserFavoriteProduct> UserFavoriteProducts { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReturnRequest> ReturnRequests { get; set; }
        public DbSet<ReturnRequestLine> ReturnRequestLines { get; set; }
        public DbSet<ProductStock> ProductStocks { get; set; }

        public RepositoryContext(DbContextOptions<RepositoryContext> options) : base(options) //RepositoryContext ten nesne üretmek isteyen DbContextOptions ile gelcek ve biz de onu base e göndereceğiz yani DbContext e.
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) //eğer veri yoksa bunlar veritabanına eklenecek ama veri varsa dokunmayacak.
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        }
    }
}
