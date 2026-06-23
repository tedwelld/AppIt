using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Core.AppServices
{
    public class NightAuditService : INightAuditService
    {
        private readonly AppItDbContext _context;

        public NightAuditService(AppItDbContext context) => _context = context;

        public async Task<int> ProcessReservationProductsAsync(DateTime? auditDate = null)
        {
            var cutoff = (auditDate ?? DateTime.UtcNow).Date;
            var items = await _context.ReservationServiceItems
                .Include(i => i.Reservation)
                .Where(i => i.ActivityDate != null && i.ActivityDate < cutoff && !i.IsPostedToJournal)
                .ToListAsync();

            foreach (var item in items)
            {
                if (item.Reservation != null && item.Reservation.TravelStatus != "Travelled"
                    && item.Reservation.Status is "Confirmed" or "Closed")
                {
                    item.Reservation.TravelStatus = "Travelled";
                }
            }

            await _context.SaveChangesAsync();
            return items.Count;
        }
    }
}
