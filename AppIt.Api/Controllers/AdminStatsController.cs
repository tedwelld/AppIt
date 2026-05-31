using AppIt.Core.DTOs;
using AppIt.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Api.Controllers
{
    [Authorize(Roles = "super,admin")]
    [ApiController]
    [Route("api/admin/stats")]
    public class AdminStatsController : ControllerBase
    {
        private readonly AppItDbContext _context;

        public AdminStatsController(AppItDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? range)
        {
            var normalized = string.IsNullOrWhiteSpace(range) ? "weekly" : range.ToLowerInvariant();
            var (start, buckets) = ResolveRange(normalized);

            var reservationsQuery = _context.Reservations.AsNoTracking();
            var invoicesQuery = _context.Invoices
                .AsNoTracking()
                .Where(i => i.Status == null
                    || (i.Status.ToLower() != "cancelled"
                        && i.Status.ToLower() != "canceled"
                        && i.Status.ToLower() != "void"
                        && i.Status.ToLower() != "deleted"));
            var customersQuery = _context.Customers.AsNoTracking();

            if (start.HasValue)
            {
                reservationsQuery = reservationsQuery.Where(r => r.CreatedDate >= start.Value);
                invoicesQuery = invoicesQuery.Where(i => i.IssuedDate >= start.Value);
                customersQuery = customersQuery.Where(c => c.DateUpdated >= start.Value);
            }

            var totalBookings = await reservationsQuery.CountAsync();
            var totalSales = await invoicesQuery.CountAsync();
            var totalEarnings = await invoicesQuery.SumAsync(i => (decimal?)i.TotalAmount) ?? 0m;
            var totalCustomers = await customersQuery.CountAsync();

            var totalAccounts = await _context.Accounts.AsNoTracking().CountAsync();
            var totalReservations = await _context.Reservations.AsNoTracking().CountAsync();
            var totalInvoices = await _context.Invoices.AsNoTracking().CountAsync();
            var totalPayments = await _context.Payments.AsNoTracking().CountAsync();
            var totalRevenue = await _context.Payments.AsNoTracking().SumAsync(p => (decimal?)p.Amount) ?? 0m;
            var pendingPayments = await _context.Payments.AsNoTracking().CountAsync(p => p.Status != null && p.Status.ToLower() == "pending");
            var activeVouchers = await _context.Vouchers.AsNoTracking().CountAsync();

            var trend = await BuildTrendAsync(start, buckets);

            var dto = new AdminStatsDto
            {
                Range = normalized,
                TotalAccounts = totalAccounts,
                TotalReservations = totalReservations,
                TotalInvoices = totalInvoices,
                TotalPayments = totalPayments,
                TotalRevenue = totalRevenue,
                TotalSales = totalSales,
                TotalBookings = totalBookings,
                TotalEarnings = totalEarnings,
                TotalCustomers = totalCustomers,
                PendingPayments = pendingPayments,
                ActiveVouchers = activeVouchers,
                Trend = trend
            };

            return Ok(dto);
        }

        private static (DateTime? start, int buckets) ResolveRange(string range)
        {
            return range switch
            {
                "daily" => (DateTime.UtcNow.Date, 8),
                "weekly" => (DateTime.UtcNow.Date.AddDays(-7), 7),
                "monthly" => (DateTime.UtcNow.Date.AddDays(-30), 10),
                "quarterly" => (DateTime.UtcNow.Date.AddMonths(-3), 12),
                "yearly" => (DateTime.UtcNow.Date.AddYears(-1), 12),
                _ => (DateTime.UtcNow.Date.AddDays(-7), 7)
            };
        }

        private async Task<List<int>> BuildTrendAsync(DateTime? start, int buckets)
        {
            var trend = new List<int>();
            if (!start.HasValue)
            {
                return trend;
            }

            var rangeSeconds = (DateTime.UtcNow - start.Value).TotalSeconds;
            var bucketSeconds = Math.Max(rangeSeconds / buckets, 1);

            var invoiceDates = await _context.Invoices
                .AsNoTracking()
                .Where(i => i.IssuedDate >= start.Value)
                .Where(i => i.Status == null
                    || (i.Status.ToLower() != "cancelled"
                        && i.Status.ToLower() != "canceled"
                        && i.Status.ToLower() != "void"
                        && i.Status.ToLower() != "deleted"))
                .Select(i => i.IssuedDate)
                .ToListAsync();

            for (var i = 0; i < buckets; i++)
            {
                var bucketStart = start.Value.AddSeconds(bucketSeconds * i);
                var bucketEnd = start.Value.AddSeconds(bucketSeconds * (i + 1));
                var count = invoiceDates.Count(issuedAt => issuedAt >= bucketStart && issuedAt < bucketEnd);
                trend.Add(count == 0 ? 1 : count);
            }

            return trend;
        }
    }
}
