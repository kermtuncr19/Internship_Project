using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repositories;

namespace StoreApp.Infrastructure.Extensions
{
    public static class ApplicationExtension
    {
        public static void ConfigureAndCheckMigration(this IApplicationBuilder app)
        {
            RepositoryContext context = app
                .ApplicationServices
                .CreateScope()
                .ServiceProvider
                .GetRequiredService<RepositoryContext>();

            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }
        }
        public static void ConfigureLocalization(this WebApplication app)
        {
            app.UseRequestLocalization(options =>
            {
                options.AddSupportedCultures("tr-TR")
                    .AddSupportedUICultures("tr-TR")
                    .SetDefaultCulture("tr-TR");
            });
        }
        public static async Task ConfigureDefaultAdminUser(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            var adminUsername = config["ADMIN:USERNAME"] ?? "Admin";
            var adminEmail = config["ADMIN:EMAIL"] ?? "admin@local";
            var adminPassword = config["ADMIN:PASSWORD"];

            // Prod’da şifre ENV'den gelmiyorsa admin oluşturma (güvenlik)
            if (string.IsNullOrWhiteSpace(adminPassword))
                return;

            // Önce Admin rolü garanti olsun
            const string adminRole = "Admin";
            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                var roleCreate = await roleManager.CreateAsync(new IdentityRole(adminRole));
                if (!roleCreate.Succeeded)
                    throw new Exception("Admin rolü oluşturulamadı: " + string.Join(", ", roleCreate.Errors.Select(e => e.Description)));
            }

            // UserName veya Email’den yakala
            var user = await userManager.FindByNameAsync(adminUsername)
                       ?? await userManager.FindByEmailAsync(adminEmail);

            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = adminUsername,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    PhoneNumber = "5512311426"
                };

                var createResult = await userManager.CreateAsync(user, adminPassword);
                if (!createResult.Succeeded)
                    throw new Exception("Admin kullanıcı oluşturulamadı: " + string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }

            // Admin rolünü garanti et
            if (!await userManager.IsInRoleAsync(user, adminRole))
            {
                var addRole = await userManager.AddToRoleAsync(user, adminRole);
                if (!addRole.Succeeded)
                    throw new Exception("Admin rolü atanamadı: " + string.Join(", ", addRole.Errors.Select(e => e.Description)));
            }
        }

    }
}