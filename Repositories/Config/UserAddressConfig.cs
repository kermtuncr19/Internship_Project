using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositories.Config
{
    public class UserAddressConfig : IEntityTypeConfiguration<UserAddress>
    {
        public void Configure(EntityTypeBuilder<UserAddress> b)
        {
            b.HasKey(x => x.Id);

            b.HasOne(x => x.User)
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            b.Property(x => x.Label).HasMaxLength(100);
            b.Property(x => x.City).HasMaxLength(100).IsRequired();
            b.Property(x => x.District).HasMaxLength(100).IsRequired();
            b.Property(x => x.Neighborhood).HasMaxLength(150).IsRequired();
            b.Property(x => x.AddressLine).HasMaxLength(500).IsRequired();

            // b.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'"); // opsiyonel
        }
    }
}
