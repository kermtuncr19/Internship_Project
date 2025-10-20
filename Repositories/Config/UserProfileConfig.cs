using Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositories.Config
{
    public class UserProfileConfig : IEntityTypeConfiguration<UserProfile>
    {
        public void Configure(EntityTypeBuilder<UserProfile> b)
        {
            b.HasKey(x => x.Id);

            // Her kullanıcı için tek profil
            b.HasIndex(x => x.UserId).IsUnique();

            // FK: User silinirse profil de silinsin
            b.HasOne(x => x.User)
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            // Alan kısıtları (opsiyonel ama iyi pratik)
            b.Property(x => x.FullName).HasMaxLength(150);
            b.Property(x => x.PhoneNumber).HasMaxLength(32);
            b.Property(x => x.AvatarUrl).HasMaxLength(512);

            // 🔸 Doğum tarihi sadece 'date' (timezone sorunları biter)
            b.Property(x => x.BirthDate)
             .HasColumnType("date"); // PostgreSQL 'date'
        }
    }
}
