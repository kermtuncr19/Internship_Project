using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
    public class RepositoryContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public RepositoryContext(DbContextOptions<RepositoryContext> options) : base(options) //RepositoryContext ten nesne üretmek isteyen DbContextOptions ile gelcek ve biz de onu base e göndereceğiz yani DbContext e.
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) //eğer veri yoksa bunlar veritabanına eklenecek ama veri varsa dokunmayacak.
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Product>()
            .HasData(
                new Product() { Id = 1, ProductName = "Computer", Price = 19000 },
                new Product() { Id = 2, ProductName = "Keyboard", Price = 1000 },
                new Product() { Id = 3, ProductName = "Mouse", Price = 900 },
                new Product() { Id = 4, ProductName = "Monitor", Price = 10000 },
                new Product() { Id = 5, ProductName = "Deck", Price = 5000 }
            );
            modelBuilder.Entity<Category>()
            .HasData(
                new Category() { CategoryId = 1, CategoryName = "Football" },
                new Category() { CategoryId = 2, CategoryName = "Basketball" },
                 new Category() { CategoryId = 3, CategoryName = "Volleyball" }
            );
        }
    }
}
