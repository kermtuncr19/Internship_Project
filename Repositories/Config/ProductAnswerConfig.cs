using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositories.Config
{
    public class ProductAnswerConfig : IEntityTypeConfiguration<ProductAnswer>
    {
        public void Configure(EntityTypeBuilder<ProductAnswer> builder)
        {
            builder.HasKey(x => x.ProductAnswerId);

            builder.Property(x => x.AnswerText)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.HasIndex(x => x.ProductQuestionId)
                .IsUnique(); // 1 soruya 1 cevap

            builder.HasOne(x => x.AdminUser)
                .WithMany()
                .HasForeignKey(x => x.AdminUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
