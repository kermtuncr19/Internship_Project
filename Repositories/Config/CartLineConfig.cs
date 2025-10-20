using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositories.Config
{
    public class CartLineConfig : IEntityTypeConfiguration<CartLine>
    {
        public void Configure(EntityTypeBuilder<CartLine> b)
        {
            b.HasKey(x => x.CartLineId);
            b.Property(x => x.CartLineId).ValueGeneratedOnAdd();

            b.Property(x => x.Quantity).IsRequired();

            b.HasOne(x => x.Order)
             .WithMany(o => o.Lines)
             .HasForeignKey(x => x.OrderId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Product)
             .WithMany()
             .HasForeignKey(x => x.ProductId)
             .OnDelete(DeleteBehavior.Restrict);

            // Aynı siparişte aynı ürün+beden bir kez olsun istiyorsan:
            // b.HasIndex(x => new { x.OrderId, x.ProductId, x.Size }).IsUnique();
        }
    }
}
