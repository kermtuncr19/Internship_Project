using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositories.Config
{
    public class ReturnRequestLineConfig : IEntityTypeConfiguration<ReturnRequestLine>
    {
        public void Configure(EntityTypeBuilder<ReturnRequestLine> builder)
        {
            builder.ToTable("ReturnRequestLines");

            builder.HasKey(l => l.ReturnRequestLineId);       // key adını kendi entity’ine göre ayarla
            builder.Property(l => l.ReturnRequestLineId).ValueGeneratedOnAdd();

            builder.Property(l => l.ReturnRequestId).IsRequired();

            // ReturnRequestLine -> ReturnRequest (N:1)
            builder.HasOne(l => l.ReturnRequest)
                   .WithMany(r => r.Lines)
                   .HasForeignKey(l => l.ReturnRequestId)
                   .OnDelete(DeleteBehavior.Cascade); // satırlar, talep silinirse silinsin
        }
    }
}
