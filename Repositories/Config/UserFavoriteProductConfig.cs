using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositories.Config
{
    public class UserFavoriteProductConfig : IEntityTypeConfiguration<UserFavoriteProduct>
    {
        public void Configure(EntityTypeBuilder<UserFavoriteProduct> b)
        {
            b.HasKey(x => new { x.UserId, x.ProductId });

            b.HasOne(x => x.User)
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Product)
             .WithMany()
             .HasForeignKey(x => x.ProductId)
             .OnDelete(DeleteBehavior.Cascade);

            // b.Property(x => x.AddedAt).HasDefaultValueSql("now() at time zone 'utc'"); // opsiyonel
        }
    }
}
