using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Api.SeedData
{
    public static class InitialDataSeeder
    {
        public static async Task SeedAsync(AppItDbContext dbContext)
        {
            await dbContext.Database.MigrateAsync();

            await SeedRolesAsync(dbContext);
            await SeedAdminAccountAsync(dbContext);
            await SeedProductsAsync(dbContext);
            await SeedAccommodationsAsync(dbContext);
            await SeedActivitiesAsync(dbContext);
        }

        private static async Task SeedRolesAsync(AppItDbContext dbContext)
        {
            var roleNames = new[] { "regular", "super", "admin" };
            var existing = await dbContext.Roles.Select(r => r.Name.ToLower()).ToListAsync();
            var missing = roleNames.Where(name => !existing.Contains(name)).Select(name => new Role { Name = name }).ToList();

            if (missing.Count == 0)
            {
                return;
            }

            dbContext.Roles.AddRange(missing);
            await dbContext.SaveChangesAsync();
        }

        private static async Task SeedProductsAsync(AppItDbContext dbContext)
        {
            var products = new List<Product>
            {
                new() { Name = "Bungee", Category = "Adventure", Description = "Leap from the gorge with expert guides.", BasePriceUsd = 120m, IsActive = true, CreatedDate = DateTime.UtcNow },
                new() { Name = "Swing", Category = "Adventure", Description = "A giant swing over the river.", BasePriceUsd = 95m, IsActive = true, CreatedDate = DateTime.UtcNow },
                new() { Name = "Bridge Tours", Category = "Tours", Description = "Guided bridge walk and history.", BasePriceUsd = 55m, IsActive = true, CreatedDate = DateTime.UtcNow },
                new() { Name = "View of the Falls", Category = "Scenic", Description = "Sunrise viewpoints and photo stops.", BasePriceUsd = 35m, IsActive = true, CreatedDate = DateTime.UtcNow },
                new() { Name = "Boat Cruise", Category = "Water", Description = "Sunset cruise with refreshments.", BasePriceUsd = 80m, IsActive = true, CreatedDate = DateTime.UtcNow }
            };

            var existingNames = await dbContext.Products.Select(p => p.Name.ToLower()).ToListAsync();
            var missing = products.Where(product => !existingNames.Contains(product.Name.ToLower())).ToList();
            if (missing.Count == 0)
            {
                return;
            }

            dbContext.Products.AddRange(missing);
            await dbContext.SaveChangesAsync();
        }

        private static async Task SeedAdminAccountAsync(AppItDbContext dbContext)
        {
            var superRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == "super");
            if (superRole == null)
            {
                return;
            }

            var exists = await dbContext.Accounts.AnyAsync(a => a.Email.ToLower() == "admin@appit.com");
            if (exists)
            {
                return;
            }

            dbContext.Accounts.Add(new Account
            {
                FirstName = "System",
                LastName = "Administrator",
                Email = "admin@appit.com",
                Phone = "+263 77 000 0000",
                PreferredCurrency = "USD",
                RoleId = superRole.RoleId,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();
        }

        private static async Task SeedAccommodationsAsync(AppItDbContext dbContext)
        {
            var rooms = new List<Accommodation>
            {
                new() { Type = "Single", Description = "Cozy single room with balcony.", Capacity = 1, BasePriceUsd = 75m, IsActive = true, CreatedDate = DateTime.UtcNow },
                new() { Type = "Double", Description = "Double room with panoramic views.", Capacity = 2, BasePriceUsd = 120m, IsActive = true, CreatedDate = DateTime.UtcNow },
                new() { Type = "Express", Description = "Fast check-in business suite.", Capacity = 2, BasePriceUsd = 150m, IsActive = true, CreatedDate = DateTime.UtcNow },
                new() { Type = "Standard", Description = "Classic comfort room.", Capacity = 2, BasePriceUsd = 95m, IsActive = true, CreatedDate = DateTime.UtcNow }
            };

            var existingTypes = await dbContext.Accommodations.Select(a => a.Type.ToLower()).ToListAsync();
            var missing = rooms.Where(room => !existingTypes.Contains(room.Type.ToLower())).ToList();
            if (missing.Count == 0)
            {
                return;
            }

            dbContext.Accommodations.AddRange(missing);
            await dbContext.SaveChangesAsync();
        }

        private static async Task SeedActivitiesAsync(AppItDbContext dbContext)
        {
            var activities = new List<Activity>
            {
                new() { Name = "Bungee", Description = "High-adrenaline jump experience.", BasePriceUsd = 120m, IsActive = true, CreatedDate = DateTime.UtcNow },
                new() { Name = "Gorge Swing", Description = "Swing over the misty gorge.", BasePriceUsd = 90m, IsActive = true, CreatedDate = DateTime.UtcNow },
                new() { Name = "Bridge Tour", Description = "Architecture and history tour.", BasePriceUsd = 55m, IsActive = true, CreatedDate = DateTime.UtcNow },
                new() { Name = "Falls View", Description = "Guided falls vista walk.", BasePriceUsd = 35m, IsActive = true, CreatedDate = DateTime.UtcNow },
                new() { Name = "Boat Cruise", Description = "Sunset cruise and wildlife.", BasePriceUsd = 80m, IsActive = true, CreatedDate = DateTime.UtcNow }
            };

            var existingNames = await dbContext.Activities.Select(a => a.Name.ToLower()).ToListAsync();
            var missing = activities.Where(activity => !existingNames.Contains(activity.Name.ToLower())).ToList();
            if (missing.Count == 0)
            {
                return;
            }

            dbContext.Activities.AddRange(missing);
            await dbContext.SaveChangesAsync();
        }
    }
}
