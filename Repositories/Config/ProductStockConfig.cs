using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositories.Config
{
    public class ProductStockConfig : IEntityTypeConfiguration<ProductStock>
    {
        public void Configure(EntityTypeBuilder<ProductStock> builder)
        {
            builder.HasKey(ps => ps.ProductStockId);
            
            builder.Property(ps => ps.Quantity)
                .IsRequired()
                .HasDefaultValue(0);
                
            builder.Property(ps => ps.Size)
                .HasMaxLength(10);
                
            // Unique constraint: Aynı üründe aynı beden tekrar edilemez
            builder.HasIndex(ps => new { ps.ProductId, ps.Size })
                .IsUnique();
                
            
        }
    }
}