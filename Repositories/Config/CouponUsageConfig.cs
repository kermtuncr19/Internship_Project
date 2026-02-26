using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositories.Config
{
    public class CouponUsageConfig : IEntityTypeConfiguration<CouponUsage>
    {
        public void Configure(EntityTypeBuilder<CouponUsage> builder)
        {
            builder.HasKey(x => x.CouponUsageId);

            builder.Property(x => x.UserId).IsRequired();

            builder.HasIndex(x => new { x.UserId, x.CouponId }).IsUnique(); // ✅ 1 kullanıcı 1 kupon 1 kez

            builder.HasOne(x => x.Coupon)
                   .WithMany()
                   .HasForeignKey(x => x.CouponId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}