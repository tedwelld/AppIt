using AppIt.Data.Entities;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

public class AppItDbContext : DbContext
{
    public AppItDbContext(DbContextOptions<AppItDbContext> options)
        : base(options) { }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Accommodation> Accommodations { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<CustomerType> CustomerTypes { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Feature> Features { get; set; }
    public DbSet<FeaturePermission> FeaturePermissions { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ReportSnapshot> ReportSnapshots { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<ReservationServiceItem> ReservationServiceItems { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<RoleFeature> RoleFeatures { get; set; }
    public DbSet<RoleFeaturePermission> RoleFeaturePermissions { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<SupportMessage> SupportMessages { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Voucher> Vouchers { get; set; }
    public DbSet<IdempotencyRecord> IdempotencyRecords { get; set; }
    public DbSet<Currency> Currencies { get; set; }
    public DbSet<ExchangeRate> ExchangeRates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.Email).IsRequired().HasMaxLength(150);
            b.Property(e => e.PasswordHash).IsRequired().HasMaxLength(500);
            b.HasIndex(e => e.Email).IsUnique();

            b.HasOne(e => e.Role)
                .WithMany(r => r.Accounts)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Company>(b =>
        {
            b.HasKey(e => e.CompanyId);
        });

        modelBuilder.Entity<Customer>(b =>
        {
            b.HasKey(e => e.Id);

            b.HasOne(e => e.Agent)
                .WithMany(c => c.Customers)
                .HasForeignKey(e => e.AgentCompanyId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<CustomerType>(b =>
        {
            b.HasKey(e => e.Id);
            b.HasIndex(e => e.CustomerId).IsUnique();

            b.HasOne(e => e.Customer)
                .WithOne(c => c.CustomerType)
                .HasForeignKey<CustomerType>(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Department>(b =>
        {
            b.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Feature>(b =>
        {
            b.HasKey(e => e.Id);

            b.HasOne(e => e.Permission)
                .WithMany(p => p.Features)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<FeaturePermission>(b =>
        {
            b.HasKey(e => e.FeaturePermissionId);
            b.HasIndex(e => new { e.FeatureId, e.PermissionId }).IsUnique();

            b.HasOne(e => e.Feature)
                .WithMany(f => f.FeaturePermissions)
                .HasForeignKey(e => e.FeatureId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(e => e.Permission)
                .WithMany(p => p.FeaturePermissions)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Invoice>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.TotalAmount).HasPrecision(18, 2);

            b.HasOne(e => e.Reservation)
                .WithMany(r => r.Invoices)
                .HasForeignKey(e => e.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Payment>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.Amount).HasPrecision(18, 2);

            b.HasOne(e => e.Invoice)
                .WithMany()
                .HasForeignKey(e => e.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Notification>(b =>
        {
            b.HasKey(e => e.Id);

            b.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ReportSnapshot>(b =>
        {
            b.ToTable("ReportSnapshots");
            b.HasKey(e => e.Id);

            b.HasOne(e => e.GeneratedByUser)
                .WithMany()
                .HasForeignKey(e => e.GeneratedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Reservation>(b =>
        {
            b.HasKey(e => e.ReservationId);
            b.Property(e => e.TotalAmount).HasPrecision(18, 2);
            b.Property(e => e.CurrencyExchangeRate).HasPrecision(18, 6);
            b.Property(e => e.Vat).HasPrecision(18, 2);

            b.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasOne(e => e.Customer)
                .WithMany(c => c.Reservations)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasOne(e => e.CustomerType)
                .WithMany(c => c.Reservations)
                .HasForeignKey(e => e.CustomerTypeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ReservationServiceItem>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.UnitPrice).HasPrecision(18, 2);
            b.Property(e => e.TotalPrice).HasPrecision(18, 2);
            b.HasIndex(e => e.ReservationId);

            b.HasOne(e => e.Reservation)
                .WithMany(r => r.ServiceItems)
                .HasForeignKey(e => e.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Role>(b =>
        {
            b.HasKey(e => e.RoleId);
        });

        modelBuilder.Entity<RoleFeature>(b =>
        {
            b.HasKey(e => e.RoleFeatureId);
            b.HasIndex(e => new { e.RoleId, e.FeatureId }).IsUnique();

            b.HasOne(e => e.Role)
                .WithMany(r => r.RoleFeatures)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(e => e.Feature)
                .WithMany(f => f.RoleFeatures)
                .HasForeignKey(e => e.FeatureId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RoleFeaturePermission>(b =>
        {
            b.HasKey(e => e.RoleFeaturePermissionId);
            b.HasIndex(e => new { e.RoleId, e.FeatureId, e.PermissionId }).IsUnique();

            b.HasOne(e => e.Role)
                .WithMany(r => r.RoleFeaturePermissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(e => e.Feature)
                .WithMany(f => f.RoleFeaturePermissions)
                .HasForeignKey(e => e.FeatureId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(e => e.Permission)
                .WithMany(p => p.RoleFeaturePermissions)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Voucher>(b =>
        {
            b.HasKey(e => e.Id);
            b.HasIndex(e => e.Code).IsUnique();

            b.HasOne(e => e.Reservation)
                .WithMany()
                .HasForeignKey(e => e.ReservationId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<RefreshToken>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.TokenHash).IsRequired().HasMaxLength(128);
            b.HasIndex(e => e.TokenHash).IsUnique();

            b.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PasswordResetToken>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.TokenHash).IsRequired().HasMaxLength(128);
            b.HasIndex(e => e.TokenHash).IsUnique();
            b.HasIndex(e => new { e.AccountId, e.ExpiresAtUtc });

            b.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<IdempotencyRecord>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.Endpoint).IsRequired().HasMaxLength(150);
            b.Property(e => e.IdempotencyKey).IsRequired().HasMaxLength(120);
            b.Property(e => e.RequestHash).IsRequired().HasMaxLength(128);
            b.Property(e => e.ResponseBody).IsRequired();
            b.HasIndex(e => new { e.Endpoint, e.IdempotencyKey }).IsUnique();
            b.HasIndex(e => e.ExpiresAtUtc);
        });

        modelBuilder.Entity<Product>(b =>
        {
            b.Property(e => e.BasePriceUsd).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Accommodation>(b =>
        {
            b.Property(e => e.BasePriceUsd).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Activity>(b =>
        {
            b.Property(e => e.BasePriceUsd).HasPrecision(18, 2);
        });
    }
}
