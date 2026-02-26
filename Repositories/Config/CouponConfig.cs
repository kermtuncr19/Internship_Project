using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositories.Config
{
    public class CouponConfig : IEntityTypeConfiguration<Coupon>
    {
        public void Configure(EntityTypeBuilder<Coupon> builder)
        {
            builder.HasKey(x => x.CouponId);
            builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
            builder.HasIndex(x => x.Code).IsUnique();

            builder.Property(x => x.Percent).HasPrecision(5, 2);
            builder.Property(x => x.MinCartTotal).HasPrecision(18, 2);
        }
    }
}