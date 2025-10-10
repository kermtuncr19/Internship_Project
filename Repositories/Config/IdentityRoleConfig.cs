using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.AspNetCore.Identity;

namespace Repositories.Config
{
    public class IdentityRoleConfig : IEntityTypeConfiguration<IdentityRole>
    {
        public void Configure(EntityTypeBuilder<IdentityRole> builder)
        {
            builder.HasData(
                new IdentityRole() { Id="1", Name = "User", NormalizedName = "USER" },

                new IdentityRole() { Id="2", Name = "Editor", NormalizedName = "EDITOR" },

                new IdentityRole() { Id="3", Name = "Admin", NormalizedName = "ADMIN" }

            );
        }
    }
}