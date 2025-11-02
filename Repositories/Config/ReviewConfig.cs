using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositories.Config
{
    public class ReviewConfig : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasKey(r => r.ReviewId);
            
            builder.Property(r => r.Rating)
                .IsRequired();
            
            builder.Property(r => r.Comment)
                .HasMaxLength(1000);
            
            builder.Property(r => r.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");
            
            builder.Property(r => r.IsApproved)
                .IsRequired()
                .HasDefaultValue(false);
            
            builder.Property(r => r.ShowFullName)
                .IsRequired()
                .HasDefaultValue(false);
            
            // Product ilişkisi
            builder.HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // User ilişkisi
            builder.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Order ilişkisi
            builder.HasOne(r => r.Order)
                .WithMany()
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Unique constraint - bir kullanıcı bir siparişte bir ürüne sadece 1 yorum yapabilir
            builder.HasIndex(r => new { r.ProductId, r.OrderId, r.UserId })
                .IsUnique()
                .HasDatabaseName("IX_Review_ProductOrderUser");
            
            // Performans için index'ler
            builder.HasIndex(r => r.ProductId);
            builder.HasIndex(r => r.UserId);
            builder.HasIndex(r => r.IsApproved);
        }
    }
}