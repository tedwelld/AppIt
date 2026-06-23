using Microsoft.EntityFrameworkCore;

namespace AppIt.Api.SeedData
{
    /// <summary>
    /// Hard-deletes all operational/business data while preserving seeded roles and the default admin account.
    /// Trigger with APPIT_PURGE_OPERATIONAL_DATA=true or Data:PurgeOperationalOnStartup in config.
    /// </summary>
    public static class OperationalDataCleaner
    {
        private const string AdminEmail = "admin@appit.com";

        public static async Task PurgeAllAsync(AppItDbContext dbContext, ILogger? logger = null)
        {
            await BookingDataCleaner.PurgeAllAsync(dbContext, logger);

            var specialPrices = await dbContext.SpecialProductPrices.ExecuteDeleteAsync();
            var servicePrices = await dbContext.ServicePrices.ExecuteDeleteAsync();
            var products = await dbContext.Products.ExecuteDeleteAsync();
            var activities = await dbContext.Activities.ExecuteDeleteAsync();
            var tours = await dbContext.Tours.ExecuteDeleteAsync();
            var transfers = await dbContext.Transfers.ExecuteDeleteAsync();
            var accommodations = await dbContext.Accommodations.ExecuteDeleteAsync();
            var subCategories = await dbContext.ProductSubCategories.ExecuteDeleteAsync();
            var categories = await dbContext.ProductCategories.ExecuteDeleteAsync();
            var customerTypes = await dbContext.CustomerTypes.ExecuteDeleteAsync();
            var customers = await dbContext.Customers.ExecuteDeleteAsync();
            var consultants = await dbContext.Consultants.ExecuteDeleteAsync();
            var supportMessages = await dbContext.SupportMessages.ExecuteDeleteAsync();
            var notifications = await dbContext.Notifications.ExecuteDeleteAsync();
            var auditLogs = await dbContext.AuditLogs.ExecuteDeleteAsync();
            var reportSnapshots = await dbContext.ReportSnapshots.ExecuteDeleteAsync();
            var dayEnds = await dbContext.DayEnds.ExecuteDeleteAsync();
            var idempotency = await dbContext.IdempotencyRecords.ExecuteDeleteAsync();
            var exchangeRates = await dbContext.ExchangeRates.ExecuteDeleteAsync();
            var currencies = await dbContext.Currencies.ExecuteDeleteAsync();
            var suppliers = await dbContext.Suppliers.ExecuteDeleteAsync();
            var companies = await dbContext.Companies.ExecuteDeleteAsync();
            var departments = await dbContext.Departments.ExecuteDeleteAsync();

            var adminId = await dbContext.Accounts
                .AsNoTracking()
                .Where(a => a.Email.ToLower() == AdminEmail)
                .Select(a => (int?)a.Id)
                .FirstOrDefaultAsync();

            var refreshTokens = await dbContext.RefreshTokens
                .Where(t => adminId == null || t.AccountId != adminId)
                .ExecuteDeleteAsync();
            var passwordResetTokens = await dbContext.PasswordResetTokens
                .Where(t => adminId == null || t.AccountId != adminId)
                .ExecuteDeleteAsync();
            await dbContext.UserProfiles.ExecuteDeleteAsync();
            var otherAccounts = await dbContext.Accounts
                .Where(a => a.Email.ToLower() != AdminEmail)
                .ExecuteDeleteAsync();

            logger?.LogWarning(
                "Operational data purge complete. Removed catalog ({Products} products, {Categories} categories, {ServicePrices} service prices), " +
                "{Customers} customers, {Companies} companies, {OtherAccounts} non-admin accounts. Admin login preserved.",
                products,
                categories,
                servicePrices,
                customers,
                companies,
                otherAccounts);
        }
    }
}
