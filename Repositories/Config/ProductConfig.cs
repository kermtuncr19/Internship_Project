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

            // ✅ Images koleksiyonu için ilişki
            builder.HasMany(p => p.Images)
                .WithOne(pi => pi.Product)
                .HasForeignKey(pi => pi.ProductId);

            builder.HasMany(p => p.Stocks)
                .WithOne(s => s.Product)
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Questions)
                .WithOne(q => q.Product)
                .HasForeignKey(q => q.ProductId)
                .OnDelete(DeleteBehavior.Cascade);


            builder
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.HasData(
                new Product() { ProductId = 1, CategoryId = 1, ImageUrl = "/images/forma3.jpg", ProductName = "Fenerbahçe 2025/26 Lacivert Forma", Price = 4249, ShowCase = true },
                new Product() { ProductId = 2, CategoryId = 1, ImageUrl = "/images/2.jpg", ProductName = "Fenerbahçe 2025/26 Çubuklu Forma", Price = 4249, ShowCase = true },
                new Product() { ProductId = 3, CategoryId = 1, ImageUrl = "/images/3.jpg", ProductName = "Fenerbahçe 2025/26 Sarı Forma", Price = 4249, ShowCase = true },
                new Product() { ProductId = 4, CategoryId = 2, ImageUrl = "/images/4.jpg", ProductName = "Fenerbahçe Beko 2025/26 Adidas Erkek Çubuklu Forma", Price = 3799, ShowCase = false },
                new Product() { ProductId = 5, CategoryId = 2, ImageUrl = "/images/5.jpg", ProductName = "Fenerbahçe Beko 2025/26 Adidas Lacivert Erkek Maç Şortu", Price = 2499, ShowCase = false },
                new Product() { ProductId = 6, CategoryId = 3, ImageUrl = "/images/6.jpg", ProductName = "Fenerbahçe Medicana 24/25 Çubuklu Kadın Voleybol Forma", Price = 1499, ShowCase = false },
                new Product() { ProductId = 7, CategoryId = 2, ImageUrl = "/images/9.jpg", ProductName = "Fenerbahçe Basketbol Üç Kupa Tek Şampiyon Sarı Tshirt", Price = 899, ShowCase = false },
                new Product() { ProductId = 8, CategoryId = 1, ImageUrl = "/images/8.jpg", ProductName = "Fenerbahçe 2025/26 Antrasit Kaleci Forma", Price = 5399, ShowCase = false },
                new Product() { ProductId = 9, CategoryId = 2, ImageUrl = "/images/10.jpg", ProductName = "Fenerbahçe 24/25 EuroLeague Basketbol Şampiyonluk Kupa Anahtarlık", Price = 279, ShowCase = false }
            );
        }
    }
}