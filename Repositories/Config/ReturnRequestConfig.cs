using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositories.Config
{
    public class ReturnRequestConfig : IEntityTypeConfiguration<ReturnRequest>
    {
        public void Configure(EntityTypeBuilder<ReturnRequest> builder)
        {
            builder.ToTable("ReturnRequests");

            builder.HasKey(r => r.ReturnRequestId);           // key adını kendi entity’ine göre ayarla
            builder.Property(r => r.ReturnRequestId).ValueGeneratedOnAdd();

            builder.Property(r => r.OrderId).IsRequired();

            // ReturnRequest -> Order (N:1)
            builder.HasOne(r => r.Order)
                   .WithMany(o => o.ReturnRequests)
                   .HasForeignKey(r => r.OrderId)
                   .OnDelete(DeleteBehavior.Restrict); // istersen Cascade/NoAction
        }
    }
}
