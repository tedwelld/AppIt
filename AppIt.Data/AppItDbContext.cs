using AppIt.Data.Entities;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

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
    public DbSet<FeaturePermission> FeaturePermissions { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<CustomerType> CustomerTypes { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }

    public DbSet<ReportSnapshot> ReportSnapshots { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Company>()
            .HasKey(e => e.CompanyId);

        modelBuilder.Entity<Department>()
            .HasKey(e => e.Id);

        modelBuilder.Entity<Account>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.Email).IsRequired().HasMaxLength(150);
            b.HasIndex(e => e.Email).IsUnique();

            b.HasOne<Role>()
                .WithMany()
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });
       


        modelBuilder.Entity<ReportSnapshot>()
            .ToTable("ReportSnapshots");
    }
}
