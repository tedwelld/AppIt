using AppIt.Api.Infrastructure;
using AppIt.Core.DTOs;
using AppIt.Data.EntityModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Api.Controllers
{
    [Authorize(Roles = "super,admin")]
    [ApiController]
    [Route("api/cashier")]
    public class CashierController : ControllerBase
    {
        private readonly AppItDbContext _context;

        public CashierController(AppItDbContext context)
        {
            _context = context;
        }

        [HttpGet("exchange-rates")]
        public async Task<IActionResult> GetExchangeRates([FromQuery] ListQueryOptions query)
        {
            var paymentCurrencies = await _context.Payments
                .AsNoTracking()
                .Where(payment => payment.CurrencyCode != null && payment.CurrencyCode != "")
                .GroupBy(payment => payment.CurrencyCode)
                .Select(group => new
                {
                    Currency = group.Key,
                    TransactionCount = group.Count(),
                    TotalAmount = group.Sum(payment => payment.Amount)
                })
                .ToListAsync();

            var reservationCurrencies = await _context.Reservations
                .AsNoTracking()
                .Where(reservation => reservation.CurrencyCode != null && reservation.CurrencyCode != "")
                .Select(reservation => reservation.CurrencyCode)
                .Distinct()
                .ToListAsync();

            var rows = paymentCurrencies
                .Select(row => new CashierExchangeRateRow(
                    row.Currency,
                    1m,
                    row.TransactionCount,
                    row.TotalAmount,
                    "AppIt Base",
                    row.Currency.Equals("USD", StringComparison.OrdinalIgnoreCase) ? "Base" : "Configured"))
                .Concat(reservationCurrencies
                    .Where(currency => paymentCurrencies.All(row => !row.Currency.Equals(currency, StringComparison.OrdinalIgnoreCase)))
                    .Select(currency => new CashierExchangeRateRow(currency, 1m, 0, 0m, "Reservation Currency", "Configured")))
                .OrderBy(row => row.Currency)
                .ToList();

            return Ok(rows.ApplyQuery(query,
                nameof(CashierExchangeRateRow.Currency),
                nameof(CashierExchangeRateRow.Source),
                nameof(CashierExchangeRateRow.Status)));
        }

        [HttpGet("bank-note-details")]
        public async Task<IActionResult> GetBankNoteDetails([FromQuery] ListQueryOptions query)
        {
            var groupedPayments = await _context.Payments
                .AsNoTracking()
                .GroupBy(payment => new { payment.CurrencyCode, payment.Method, payment.Status })
                .Select(group => new
                {
                    group.Key.CurrencyCode,
                    group.Key.Method,
                    group.Key.Status,
                    Quantity = group.Count(),
                    Total = group.Sum(payment => payment.Amount)
                })
                .ToListAsync();

            var rows = groupedPayments
                .Select(group => new CashierBankNoteRow(
                    string.IsNullOrWhiteSpace(group.CurrencyCode) ? "Unknown" : group.CurrencyCode,
                    string.IsNullOrWhiteSpace(group.Method) ? "Unknown" : group.Method,
                    string.IsNullOrWhiteSpace(group.Status) ? "Unknown" : group.Status,
                    group.Quantity,
                    group.Total))
                .OrderBy(row => row.Currency)
                .ThenBy(row => row.Method)
                .ToList();

            return Ok(rows.ApplyQuery(query,
                nameof(CashierBankNoteRow.Currency),
                nameof(CashierBankNoteRow.Method),
                nameof(CashierBankNoteRow.Status)));
        }

        [HttpPost("bank-note-details")]
        public async Task<IActionResult> SaveBankNoteDetails([FromBody] SaveBankNoteDetailDto dto)
        {
            var entity = new BankNoteDetail
            {
                CashUpDate = dto.CashUpDate.Date,
                CurrencyCode = dto.CurrencyCode,
                Denomination = dto.Denomination,
                Quantity = dto.Quantity,
                TotalAmount = dto.TotalAmount,
                EnteredBy = User.Identity?.Name
            };
            _context.BankNoteDetails.Add(entity);
            await _context.SaveChangesAsync();
            return Ok(entity);
        }
    }

    public record SaveBankNoteDetailDto(
        DateTime CashUpDate,
        string CurrencyCode,
        string Denomination,
        int Quantity,
        decimal TotalAmount);

    public record CashierExchangeRateRow(
        string Currency,
        decimal Rate,
        int TransactionCount,
        decimal TotalAmount,
        string Source,
        string Status);

    public record CashierBankNoteRow(
        string Currency,
        string Method,
        string Status,
        int Quantity,
        decimal Total);
}
