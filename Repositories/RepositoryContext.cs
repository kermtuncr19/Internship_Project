using System.Reflection;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Config;

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
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        }
    }
}
