using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;
using AppIt.Data.EntityModels;

namespace AppIt.Data
{
    public class AppItDbContext : DbContext
    {
        public AppItDbContext(DbContextOptions<AppItDbContext> options)
            : base(options) { }

        public DbSet<Company> Companies { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RoleFeature> RoleFeatures { get; set; }
        public DbSet<RoleFeaturePermission> RoleFeaturePermissions { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<CustomerType> CustomerTypes { get; set; }
        public DbSet<FeaturePermission> FeaturePermissions { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Reservation> Reservation { get; set; }
        public DbSet<Customer> Customer { get; set; }
       
      

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =======================
            // Companies
            // =======================
            modelBuilder.Entity<Company>(b =>
            {
                b.HasKey(e => e.CompanyId);
                b.ToTable("Companies");
            });

            // =======================
            // Suppliers
            // =======================
            modelBuilder.Entity<Supplier>(b =>
            {
                b.HasKey(e => e.SupplierId);
                b.ToTable("Suppliers");
            });

            // =======================
            // Products
            // =======================
            modelBuilder.Entity<Product>(b =>
            {
                b.HasKey(e => e.ProductId);

              

                b.ToTable("Products");
            });

            // =======================
            // Permissions
            // =======================
            modelBuilder.Entity<Permission>(b =>
            {
                b.HasKey(e => e.PermissionId);
                b.ToTable("Permissions");
            });

            // =======================
            // Features
            // =======================
            modelBuilder.Entity<Feature>(b =>
            {
                b.HasKey(e => e.Id);

                b.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                b.HasMany(e => e.FeaturePermissions)
                    .WithOne(fp => fp.Feature)
                    .HasForeignKey(fp => fp.FeatureId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasMany(e => e.RoleFeatures)
                    .WithOne(rf => rf.Feature)
                    .HasForeignKey(rf => rf.FeatureId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.ToTable("Features");
            });

            // =======================
            // FeaturePermissions
            // =======================
            modelBuilder.Entity<FeaturePermission>(b =>
            {
                b.HasKey(e => e.FeaturePermissionId);

                b.HasOne(e => e.Permission)
                    .WithMany(p => p.FeaturePermissions)
                    .HasForeignKey(e => e.PermissionId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.ToTable("FeaturePermissions");
            });

            // =======================
            // Roles
            // =======================
            modelBuilder.Entity<Role>(b =>
            {
                b.HasKey(e => e.RoleId);
                b.Property(e => e.Name).IsRequired().HasMaxLength(100);
                b.ToTable("Roles");
            });

            // =======================
            // RoleFeatures
            // =======================
            modelBuilder.Entity<RoleFeature>(b =>
            {
                b.HasKey(e => e.RoleFeatureId);

                b.HasOne(e => e.Role)
                    .WithMany()
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(e => e.Feature)
                    .WithMany(f => f.RoleFeatures)
                    .HasForeignKey(e => e.FeatureId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.ToTable("RoleFeatures");
            });

            // =======================
            // RoleFeaturePermissions
            // =======================
            modelBuilder.Entity<RoleFeaturePermission>(b =>
            {
                b.HasKey(e => e.RoleFeaturePermissionId);

                b.HasOne(e => e.Role)
                    .WithMany()
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(e => e.Permission)
                    .WithMany()
                    .HasForeignKey(e => e.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.ToTable("RoleFeaturePermissions");
            });

            // =======================
            // Departments
            // =======================
            modelBuilder.Entity<Department>(b =>
            {
                b.HasKey(e => e.Id);
                b.ToTable("Departments");
            });
          

            // =======================
            // Accounts (FIXED)
            // =======================
            modelBuilder.Entity<Account>(b =>
            {
                b.HasKey(e => e.Id);

                b.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                b.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(100);

                b.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(150);

                
                // FK ONLY — no navigation to Role
                b.HasOne<Role>()
                    .WithMany()
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.ToTable("Accounts");
            });
        }
    }
}
