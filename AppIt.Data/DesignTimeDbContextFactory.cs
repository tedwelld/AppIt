using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

namespace AppIt.Data
{
    // Provides a design-time factory for EF Core tools (migrations)
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppItDbContext>
    {
        public AppItDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppItDbContext>();

            // Default to LocalDB; override by setting the CONNECTION_STRING environment variable if needed
            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
                ?? "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=AppItDb;MultipleActiveResultSets=true;TrustServerCertificate=true;";

            optionsBuilder.UseSqlServer(connectionString);
            return new AppItDbContext(optionsBuilder.Options);
        }
    }
}
