using AppIt.Core.DTOs;
using AppIt.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Api.Controllers
{
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
            var paymentsQuery = _context.Payments.AsNoTracking().Where(p => p.Status == "Paid");
            var accountsQuery = _context.Accounts.AsNoTracking();

            if (start.HasValue)
            {
                reservationsQuery = reservationsQuery.Where(r => r.CreatedDate >= start.Value);
                paymentsQuery = paymentsQuery.Where(p => p.ProcessedAt != null && p.ProcessedAt >= start.Value);
                accountsQuery = accountsQuery.Where(a => a.CreatedDate >= start.Value);
            }

            var totalBookings = await reservationsQuery.CountAsync();
            var totalSales = await paymentsQuery.CountAsync();
            var totalEarnings = await paymentsQuery.SumAsync(p => (decimal?)p.Amount) ?? 0m;
            var totalCustomers = await accountsQuery.CountAsync();

            var trend = await BuildTrendAsync(start, buckets);

            var dto = new AdminStatsDto
            {
                Range = normalized,
                TotalSales = totalSales,
                TotalBookings = totalBookings,
                TotalEarnings = totalEarnings,
                TotalCustomers = totalCustomers,
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

            var payments = await _context.Payments
                .AsNoTracking()
                .Where(p => p.Status == "Paid" && p.ProcessedAt != null && p.ProcessedAt >= start.Value)
                .Select(p => p.ProcessedAt!.Value)
                .ToListAsync();

            for (var i = 0; i < buckets; i++)
            {
                var bucketStart = start.Value.AddSeconds(bucketSeconds * i);
                var bucketEnd = start.Value.AddSeconds(bucketSeconds * (i + 1));
                var count = payments.Count(p => p >= bucketStart && p < bucketEnd);
                trend.Add(count == 0 ? 1 : count);
            }

            return trend;
        }
    }
}
