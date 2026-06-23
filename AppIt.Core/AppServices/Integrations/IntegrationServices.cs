using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AppIt.Core.AppServices.Integrations
{
    public class HConnectService : IHConnectService
    {
        private readonly AppItDbContext _context;
        private readonly ILogger<HConnectService> _logger;

        public HConnectService(AppItDbContext context, ILogger<HConnectService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<HConnectBookingReadDto>> GetPendingAsync() =>
            await _context.HConnectBookings.AsNoTracking()
                .Where(b => b.SyncStatus == "Pending" || b.SyncStatus == "Failed")
                .Select(b => new HConnectBookingReadDto
                {
                    Id = b.Id, ReservationId = b.ReservationId,
                    SyncStatus = b.SyncStatus, ErrorMessage = b.ErrorMessage
                }).ToListAsync();

        public async Task<HConnectBookingReadDto?> QueueReservationAsync(int reservationId)
        {
            var res = await _context.Reservations.AsNoTracking().FirstOrDefaultAsync(r => r.ReservationId == reservationId);
            if (res == null) return null;

            var booking = new HConnectBooking
            {
                ReservationId = reservationId,
                ArrivalDate = DateOnly.FromDateTime(DateTime.UtcNow),
                DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                GuestFirstName = res.CustomerFirstName ?? "Guest",
                GuestLastName = res.CustomerLastName ?? "Guest",
                TotalAmount = res.TotalAmount,
                SyncStatus = "Pending"
            };
            _context.HConnectBookings.Add(booking);
            await _context.SaveChangesAsync();
            return new HConnectBookingReadDto { Id = booking.Id, ReservationId = reservationId, SyncStatus = "Pending" };
        }

        public async Task<int> ProcessPendingRetriesAsync()
        {
            var pending = await _context.HConnectBookings
                .Where(b => b.SyncStatus == "Pending" || b.SyncStatus == "Failed")
                .Take(20).ToListAsync();

            foreach (var b in pending)
            {
                b.LastSyncAttempt = DateTime.UtcNow;
                b.RetryCount++;
                b.SyncStatus = "Synced";
                b.HConnectConfirmationNumber = $"HC-{b.ReservationId}";
                _logger.LogInformation("H-Connect sync simulated for reservation {Id}", b.ReservationId);
            }
            await _context.SaveChangesAsync();
            return pending.Count;
        }
    }

    public class Beds24Service : IBeds24Service
    {
        private readonly ILogger<Beds24Service> _logger;

        public Beds24Service(ILogger<Beds24Service> logger) => _logger = logger;

        public Task SyncInventoryAsync()
        {
            _logger.LogInformation("Beds24 inventory sync (stub)");
            return Task.CompletedTask;
        }

        public Task CallApiAsync()
        {
            _logger.LogInformation("Beds24 API poll (stub)");
            return Task.CompletedTask;
        }
    }

    public class SimunyeService : ISimunyeService
    {
        private readonly ILogger<SimunyeService> _logger;

        public SimunyeService(ILogger<SimunyeService> logger) => _logger = logger;

        public Task<int> ImportFromEmailAsync()
        {
            _logger.LogInformation("Simunye IMAP import (stub)");
            return Task.FromResult(0);
        }
    }
}
