using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AppIt.Data
{
    /// <summary>
    /// Design-time DbContext factory for EF Core tools (migrations, scaffolding).
    /// </summary>
    public sealed class DesignTimeDbContextFactory
        : IDesignTimeDbContextFactory<AppItDbContext>
    {
        public AppItDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppItDbContext>();

            var connectionString =
                Environment.GetEnvironmentVariable("CONNECTION_STRING")
                ?? "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=AppItDb;MultipleActiveResultSets=true;TrustServerCertificate=true;";

            optionsBuilder.UseSqlServer(connectionString);

            return new AppItDbContext(optionsBuilder.Options);
        }
    }
}
