using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositories.Config
{
    public class UserFavoriteProductConfig : IEntityTypeConfiguration<UserFavoriteProduct>
    {
        public void Configure(EntityTypeBuilder<UserFavoriteProduct> b)
        {
            // Kompozit PK
            b.HasKey(x => new { x.UserId, x.ProductId });

            // FK alanları veritabanı tarafından üretilmez
            b.Property(x => x.UserId).ValueGeneratedNever();
            b.Property(x => x.ProductId).ValueGeneratedNever();

            // İlişkiler
            b.HasOne(x => x.User)
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade)
             .IsRequired(); // UserId null değil

            b.HasOne(x => x.Product)
             .WithMany()
             .HasForeignKey(x => x.ProductId)
             .OnDelete(DeleteBehavior.Cascade)
             .IsRequired(); // ProductId null değil

            // (Opsiyonel) AddedAt için varsayılan UTC now
            // b.Property(x => x.AddedAt)
            //  .HasDefaultValueSql("CURRENT_TIMESTAMP"); // SQL Server için GETUTCDATE()
            //  // .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
