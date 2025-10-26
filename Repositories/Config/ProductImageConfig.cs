using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositories.Config
{
    public class ProductImageConfig : IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> builder)
        {
            builder.HasKey(pi => pi.ProductImageId);
            
            builder.Property(pi => pi.ImageUrl)
                .IsRequired()
                .HasMaxLength(500);
            
            builder.Property(pi => pi.DisplayOrder)
                .HasDefaultValue(0);
            
            builder.Property(pi => pi.IsMain)
                .HasDefaultValue(false);
            
            // İlişki tanımı
            builder.HasOne(pi => pi.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Seed Data - Mevcut ürünler için ana fotoğrafları ekle
            builder.HasData(
                new ProductImage { ProductImageId = 1, ProductId = 1, ImageUrl = "/images/forma3.jpg", DisplayOrder = 0, IsMain = true },
                new ProductImage { ProductImageId = 2, ProductId = 2, ImageUrl = "/images/2.jpg", DisplayOrder = 0, IsMain = true },
                new ProductImage { ProductImageId = 3, ProductId = 3, ImageUrl = "/images/3.jpg", DisplayOrder = 0, IsMain = true },
                new ProductImage { ProductImageId = 4, ProductId = 4, ImageUrl = "/images/4.jpg", DisplayOrder = 0, IsMain = true },
                new ProductImage { ProductImageId = 5, ProductId = 5, ImageUrl = "/images/5.jpg", DisplayOrder = 0, IsMain = true },
                new ProductImage { ProductImageId = 6, ProductId = 6, ImageUrl = "/images/6.jpg", DisplayOrder = 0, IsMain = true },
                new ProductImage { ProductImageId = 7, ProductId = 7, ImageUrl = "/images/9.jpg", DisplayOrder = 0, IsMain = true },
                new ProductImage { ProductImageId = 8, ProductId = 8, ImageUrl = "/images/8.jpg", DisplayOrder = 0, IsMain = true },
                new ProductImage { ProductImageId = 9, ProductId = 9, ImageUrl = "/images/10.jpg", DisplayOrder = 0, IsMain = true }
            );
        }
    }
}