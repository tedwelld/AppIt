using AppIt.Data.EntityModels;
using AppIt.Core.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Api.SeedData
{
    public static class InitialDataSeeder
    {
        public static async Task SeedAsync(AppItDbContext dbContext, IConfiguration? configuration = null, ILogger? logger = null)
        {
            // When APPIT_RESET_DATABASE=true the entire database is dropped and rebuilt so it
            // ends up empty apart from the seeded roles (super = RoleId 1) and the default
            // administrator account. All identity counters (vouchers, etc.) restart at 1.
            var resetRequested = string.Equals(
                Environment.GetEnvironmentVariable("APPIT_RESET_DATABASE"),
                "true",
                StringComparison.OrdinalIgnoreCase);

            if (resetRequested)
            {
                logger?.LogWarning("APPIT_RESET_DATABASE=true — dropping and rebuilding the database. All data except the seeded admin and roles will be removed.");
                await dbContext.Database.EnsureDeletedAsync();
            }

            if (dbContext.Database.IsRelational())
            {
                await ApplyPendingMigrationsAsync(dbContext, logger);
            }
            else
            {
                await dbContext.Database.EnsureCreatedAsync();
            }

            await SeedRolesAsync(dbContext);
            await SeedAdminAccountAsync(dbContext, configuration);
            await SeedDemoDataAsync(dbContext, configuration, logger);
            await BackfillCatalogPricingAsync(dbContext, logger);
        }

        private static async Task ApplyPendingMigrationsAsync(AppItDbContext dbContext, ILogger? logger)
        {
            try
            {
                await TryAttachLocalDbFilesAsync(dbContext, logger);
                await LogPendingMigrationsAsync(dbContext, logger);
                await dbContext.Database.MigrateAsync();
            }
            catch (SqlException ex) when (ex.Number == 5170)
            {
                if (!await TryAttachLocalDbFilesAsync(dbContext, logger))
                {
                    throw;
                }

                await LogPendingMigrationsAsync(dbContext, logger);
                await dbContext.Database.MigrateAsync();
            }
        }

        private static async Task LogPendingMigrationsAsync(AppItDbContext dbContext, ILogger? logger)
        {
            if (logger == null)
            {
                return;
            }

            var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).ToList();
            if (pendingMigrations.Count == 0)
            {
                logger.LogInformation("Database is up to date. No pending migrations found.");
                return;
            }

            logger.LogInformation(
                "Applying {MigrationCount} pending database migration(s): {Migrations}",
                pendingMigrations.Count,
                string.Join(", ", pendingMigrations));
        }

        private static async Task<bool> TryAttachLocalDbFilesAsync(AppItDbContext dbContext, ILogger? logger)
        {
            var connectionString = dbContext.Database.GetConnectionString();
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return false;
            }

            var builder = new SqlConnectionStringBuilder(connectionString);
            if (!builder.DataSource.Contains("(localdb)", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var databaseName = builder.InitialCatalog;
            if (string.IsNullOrWhiteSpace(databaseName))
            {
                return false;
            }

            var dataFile = string.IsNullOrWhiteSpace(builder.AttachDBFilename)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), $"{databaseName}.mdf")
                : Environment.ExpandEnvironmentVariables(builder.AttachDBFilename);

            if (!Path.IsPathRooted(dataFile))
            {
                dataFile = Path.GetFullPath(dataFile);
            }

            var logFile = Path.Combine(
                Path.GetDirectoryName(dataFile) ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                $"{databaseName}_log.ldf");

            if (!File.Exists(dataFile))
            {
                return false;
            }

            dbContext.Database.CloseConnection();

            var masterBuilder = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = "master",
                AttachDBFilename = string.Empty
            };

            await using var connection = new SqlConnection(masterBuilder.ConnectionString);
            await connection.OpenAsync();

            await using (var existsCommand = connection.CreateCommand())
            {
                existsCommand.CommandText = "SELECT DB_ID(@databaseName);";
                existsCommand.Parameters.AddWithValue("@databaseName", databaseName);
                var existingDatabaseId = await existsCommand.ExecuteScalarAsync();
                if (existingDatabaseId != DBNull.Value && existingDatabaseId != null)
                {
                    return true;
                }
            }

            logger?.LogWarning(
                "Database '{DatabaseName}' is not attached, but '{DataFile}' exists. Attaching the existing LocalDB files before applying migrations.",
                databaseName,
                dataFile);

            var attachFiles = $"(FILENAME = {ToSqlLiteral(dataFile)})";
            if (File.Exists(logFile))
            {
                attachFiles += $", (FILENAME = {ToSqlLiteral(logFile)})";
            }

            await using var attachCommand = connection.CreateCommand();
            attachCommand.CommandText = $"CREATE DATABASE {ToSqlIdentifier(databaseName)} ON {attachFiles} FOR ATTACH;";
            await attachCommand.ExecuteNonQueryAsync();
            SqlConnection.ClearAllPools();
            return true;
        }

        private static string ToSqlIdentifier(string value)
        {
            return $"[{value.Replace("]", "]]")}]";
        }

        private static string ToSqlLiteral(string value)
        {
            return $"N'{value.Replace("'", "''")}'";
        }

        private static async Task SeedRolesAsync(AppItDbContext dbContext)
        {
            var roleNames = AppIt.Core.Authorization.RoleCatalog.AllSeededRoles.ToArray();
            var existing = await dbContext.Roles.Select(r => r.Name.ToLower()).ToListAsync();
            foreach (var roleName in roleNames)
            {
                if (existing.Contains(roleName.ToLowerInvariant()))
                {
                    continue;
                }

                dbContext.Roles.Add(new Role { Name = roleName });
                await dbContext.SaveChangesAsync();
                existing.Add(roleName.ToLowerInvariant());
            }
        }

        private static async Task SeedAdminAccountAsync(AppItDbContext dbContext, IConfiguration? configuration)
        {
            var superRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == "super");
            if (superRole == null)
            {
                return;
            }

            var adminPassword = Environment.GetEnvironmentVariable("APPIT_ADMIN_PASSWORD")
                ?? configuration?["Auth:SeedAdminPassword"]
                ?? "Admin@2026";
            var existing = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Email.ToLower() == "admin@appit.com");
            if (existing != null)
            {
                if (string.IsNullOrWhiteSpace(existing.PasswordHash))
                {
                    existing.PasswordHash = AuthService.HashPassword(adminPassword);
                }

                existing.RoleId = superRole.RoleId;
                existing.IsActive = true;
                existing.UpdatedDate = DateTime.UtcNow;
                await dbContext.SaveChangesAsync();
                return;
            }

            dbContext.Accounts.Add(new Account
            {
                FirstName = "System",
                LastName = "Administrator",
                Email = "admin@appit.com",
                PasswordHash = AuthService.HashPassword(adminPassword),
                Phone = "+263 77 000 0000",
                PreferredCurrency = "USD",
                RoleId = superRole.RoleId,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();
        }

        private static async Task SeedDemoDataAsync(AppItDbContext dbContext, IConfiguration? configuration, ILogger? logger)
        {
            var enabled = string.Equals(
                Environment.GetEnvironmentVariable("APPIT_SEED_DEMO_DATA")
                    ?? configuration?["Seed:DemoData"],
                "true",
                StringComparison.OrdinalIgnoreCase);
            if (!enabled)
            {
                return;
            }

            if (!await dbContext.Currencies.AnyAsync(c => c.Code == "USD"))
            {
                dbContext.Currencies.Add(new Currency { Code = "USD", Name = "US Dollar", IsActive = true });
            }

            if (!await dbContext.Products.AnyAsync())
            {
                dbContext.Products.Add(new Product
                {
                    Name = "Victoria Falls Tour",
                    Description = "Guided falls experience",
                    IsActive = true,
                    BasePriceUsd = 120m
                });
            }

            if (!await dbContext.ExchangeRates.AnyAsync())
            {
                dbContext.ExchangeRates.Add(new ExchangeRate
                {
                    CurrencyCode = "ZAR",
                    Rate = 18.5m,
                    EffectiveDate = DateTime.UtcNow.Date
                });
            }

            await dbContext.SaveChangesAsync();
            logger?.LogInformation("Demo seed data applied (APPIT_SEED_DEMO_DATA=true).");
        }

        private static async Task BackfillCatalogPricingAsync(AppItDbContext dbContext, ILogger? logger)
        {
            var toursUpdated = await dbContext.Tours
                .Where(t => t.BasePriceUsd <= 0)
                .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.BasePriceUsd, 100m));
            var transfersUpdated = await dbContext.Transfers
                .Where(t => t.BasePriceUsd <= 0)
                .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.BasePriceUsd, 50m));

            if (toursUpdated + transfersUpdated > 0)
            {
                logger?.LogInformation("Backfilled BasePriceUsd for {TourCount} tours and {TransferCount} transfers.", toursUpdated, transfersUpdated);
            }
        }
    }
}
