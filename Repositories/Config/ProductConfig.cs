using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositories.Config
{
    public class ProductConfig : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(p => p.ProductId);
            builder.Property(p => p.ProductName).IsRequired();
            builder.Property(p => p.Price).IsRequired();
            
            builder.HasData(
                  new Product() { ProductId = 1, CategoryId=1, ProductName = "Fenerbahçe 2025/26 Lacivert Forma", Price = 4249 },
                new Product() { ProductId = 2,CategoryId=1, ProductName = "Fenerbahçe 2025/26 Çubuklu Forma", Price = 4249 },
                new Product() { ProductId = 3,CategoryId=1, ProductName = "Fenerbahçe 2025/26 Sarı Forma", Price = 4249 },
                new Product() { ProductId = 4,CategoryId=2, ProductName = "Fenerbahçe Beko 2025/26 Adidas Erkek Çubuklu Forma", Price = 3799 },
                new Product() { ProductId = 5,CategoryId=2, ProductName = "Fenerbahçe Beko 2025/26 Adidas Lacivert Erkek Maç Şortu", Price = 2499 },
                new Product() { ProductId = 6,CategoryId=3, ProductName = "Fenerbahçe Medicana 24/25 Çubuklu Kadın Voleybol Forma", Price = 1499 }
            );
        }
    }
}