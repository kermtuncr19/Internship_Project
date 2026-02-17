using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositories.Config
{
    public class ProductQuestionConfig : IEntityTypeConfiguration<ProductQuestion>
    {
        public void Configure(EntityTypeBuilder<ProductQuestion> builder)
        {
            builder.HasKey(x => x.ProductQuestionId);

            builder.Property(x => x.QuestionText)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.HasOne(x => x.Product)
                .WithMany(p => p.Questions)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Answer 1-1 (optional)
            builder.HasOne(x => x.Answer)
                .WithOne(a => a.Question)
                .HasForeignKey<ProductAnswer>(a => a.ProductQuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.ProductId);
            builder.HasIndex(x => x.CreatedAt);
        }
    }
}
