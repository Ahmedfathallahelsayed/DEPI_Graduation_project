using Domain.Entity;
using Infrastructure.Persistance;
using Infrastructure.Persistance.DbContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure
{
    public static class AppDbContextSeed
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDBContext>>();

            try
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDBContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // 1. Run migrations
                await context.Database.MigrateAsync();

                // 2. Seed Roles
                string[] roles = { "Admin", "Instructor", "Student" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                        logger.LogInformation("Role '{Role}' created.", role);
                    }
                }

                // 3. Seed Default Admin User
                var adminEmail = "ahmed.admin@upskill.com";
                var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
                if (existingAdmin == null)
                {
                    var admin = new AppUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FullName = "Ahmed Admin",
                        IsActive = true,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(admin, "Admin123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(admin, "Admin");
                        logger.LogInformation("Admin user '{Email}' created.", adminEmail);
                    }
                }

                // 4. Seed Categories (Development, Business, Design, Marketing, Music)
                string[] categoryNames = { "Development", "Business", "Design", "Marketing", "Music" };
                foreach (var catName in categoryNames)
                {
                    var exists = await context.Categories.AnyAsync(c => c.Name == catName);
                    if (!exists)
                    {
                        var category = new Category
                        {
                            Name = catName,
                            Description = "", // Description is empty as requested
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        await context.Categories.AddAsync(category);
                        logger.LogInformation("Category '{Category}' seeded.", catName);
                    }
                }

                await context.SaveChangesAsync();
                logger.LogInformation("✅ Database seeding completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }
    }
}
