using Microsoft.EntityFrameworkCore;

namespace AppIt.Api.SeedData
{
    /// <summary>
    /// Removes reservation/booking transactional data (and related finance rows).
    /// Used when APPIT_PURGE_BOOKINGS=true (or Bookings:PurgeOnStartup in config).
    /// </summary>
    public static class BookingDataCleaner
    {
        public static async Task PurgeAllAsync(AppItDbContext dbContext, ILogger? logger = null)
        {
            var reservationCount = await dbContext.Reservations.CountAsync();
            if (reservationCount == 0)
            {
                return;
            }

            await dbContext.ProofOfPayments.ExecuteDeleteAsync();
            await dbContext.Refunds.ExecuteDeleteAsync();
            await dbContext.CreditNotes.ExecuteDeleteAsync();
            await dbContext.Commissions.ExecuteDeleteAsync();
            await dbContext.Vouchers.ExecuteDeleteAsync();
            await dbContext.ReservationServiceItems.ExecuteDeleteAsync();
            await dbContext.ReservationSnapshots.ExecuteDeleteAsync();
            await dbContext.Payments.ExecuteDeleteAsync();
            await dbContext.Invoices.ExecuteDeleteAsync();
            await dbContext.Reservations.ExecuteDeleteAsync();

            var testCustomersRemoved = await dbContext.Customers
                .Where(c => c.Email != null && c.Email.EndsWith("@test.local", StringComparison.OrdinalIgnoreCase))
                .ExecuteDeleteAsync();

            logger?.LogInformation(
                "Removed {ReservationCount} reservation(s) and {TestCustomerCount} test customer(s) from the database.",
                reservationCount,
                testCustomersRemoved);
        }
    }
}
