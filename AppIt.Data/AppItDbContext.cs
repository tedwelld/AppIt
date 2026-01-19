using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Data
{
    public class AppItDbContext : DbContext
    {
        public AppItDbContext(DbContextOptions<AppItDbContext> options) : base(options) { }

        public DbSet<Company> Companies { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RoleFeature> RoleFeatures { get; set; }
        public DbSet<RoleFeaturePermission> RoleFeaturePermissions { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<AccountCategory> AccountCategories { get; set; }
        public DbSet<FeaturePermission> FeaturePermissions { get; set; }
    }
}
