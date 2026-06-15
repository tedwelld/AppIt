using AppIt.Core.DTOs;
using AppIt.Core.Configuration;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using AppIt.Data.Entities;
using AppIt.Data.EntityModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AppIt.Core.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppItDbContext _context;
        private readonly IReadOnlyDictionary<string, IPaymentProvider> _providerMap;
        private readonly PaymentProviderOptions _paymentOptions;

        public PaymentService(AppItDbContext context, IEnumerable<IPaymentProvider> providers, IOptions<PaymentProviderOptions> paymentOptions)
        {
            _context = context;
            _providerMap = providers.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
            _paymentOptions = paymentOptions.Value;
        }

        public async Task<PaymentReadDto> CreateAsync(CreatePaymentDto dto)
        {
            var payment = new Payment
            {
                InvoiceId = dto.InvoiceId,
                Method = dto.Method,
                Status = dto.Status,
                TransactionReference = dto.TransactionReference,
                Amount = dto.Amount,
                CurrencyCode = string.IsNullOrWhiteSpace(dto.CurrencyCode) ? "USD" : dto.CurrencyCode,
                ProcessedAt = dto.ProcessedAt
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            await SyncInvoicePaymentStatusAsync(payment.InvoiceId);
            await SyncReservationPaymentStatusForInvoiceAsync(payment.InvoiceId);
            await _context.SaveChangesAsync();

            return ToReadDto(payment);
        }

        public async Task<ProcessPaymentResultDto> ProcessAsync(ProcessPaymentDto dto)
        {
            var existingResponse = await TryGetIdempotentResponseAsync(dto);
            if (existingResponse != null)
            {
                return existingResponse;
            }

            var invoice = await _context.Invoices.FindAsync(dto.InvoiceId);
            if (invoice == null)
            {
                var notFoundResult = new ProcessPaymentResultDto
                {
                    Success = false,
                    Status = "Failed",
                    Provider = ResolveProviderName(dto.Method),
                    Message = "Invoice not found."
                };

                await SaveIdempotentResponseAsync(dto, notFoundResult, 404);
                return notFoundResult;
            }

            var amount = dto.Amount > 0 ? dto.Amount : invoice.TotalAmount;
            var currencyCode = string.IsNullOrWhiteSpace(dto.CurrencyCode) ? invoice.CurrencyCode : dto.CurrencyCode;
            var providerName = ResolveProviderName(dto.Method);

            if (!_providerMap.TryGetValue(providerName, out var provider))
            {
                var providerMissing = new ProcessPaymentResultDto
                {
                    Success = false,
                    Status = "Failed",
                    Provider = providerName,
                    Message = $"No configured provider for method '{dto.Method}'."
                };
                await SaveIdempotentResponseAsync(dto, providerMissing, 400);
                return providerMissing;
            }

            var providerRequest = new ProcessPaymentDto
            {
                InvoiceId = dto.InvoiceId,
                Method = dto.Method,
                Amount = amount,
                CurrencyCode = currencyCode,
                ReturnUrl = dto.ReturnUrl,
                CancelUrl = dto.CancelUrl,
                IdempotencyKey = dto.IdempotencyKey
            };

            var providerResult = await provider.ProcessAsync(providerRequest);
            var activeProvider = provider;

            if (!providerResult.Success)
            {
                var failedResult = new ProcessPaymentResultDto
                {
                    Success = false,
                    Status = "Failed",
                    Provider = activeProvider.Name,
                    Message = providerResult.Message
                };
                await SaveIdempotentResponseAsync(dto, failedResult, 400);
                return failedResult;
            }

            var paymentStatus = providerResult.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase)
                || providerResult.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)
                || providerResult.Status.Equals("Captured", StringComparison.OrdinalIgnoreCase)
                ? "Paid"
                : "Pending";

            var payment = new Payment
            {
                InvoiceId = dto.InvoiceId,
                Method = dto.Method,
                Status = paymentStatus,
                TransactionReference = !string.IsNullOrWhiteSpace(dto.TransactionReference)
                    ? dto.TransactionReference.Trim()
                    : providerResult.TransactionReference,
                Amount = amount,
                CurrencyCode = currencyCode.ToUpperInvariant(),
                ProcessedAt = paymentStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase) ? DateTime.UtcNow : null
            };

            _context.Payments.Add(payment);

            if (paymentStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase))
            {
                invoice.Status = "Paid";
                invoice.IsPaid = true;
            }
            else
            {
                invoice.Status = "Pending";
                invoice.IsPaid = false;
            }

            await _context.SaveChangesAsync();

            await SyncReservationPaymentStatusForInvoiceAsync(invoice.Id);
            await _context.SaveChangesAsync();

            await NotifyAdminsAsync(invoice.Id, amount, currencyCode, payment.Method, paymentStatus, payment.TransactionReference);

            var successResult = new ProcessPaymentResultDto
            {
                Success = true,
                Provider = activeProvider.Name,
                Status = paymentStatus,
                Message = providerResult.Message,
                TransactionReference = providerResult.TransactionReference,
                RedirectUrl = providerResult.RedirectUrl,
                PaymentId = payment.Id
            };

            await SaveIdempotentResponseAsync(dto, successResult, 200);
            return successResult;
        }

        public async Task<PaymentReadDto?> UpdateAsync(UpdatePaymentDto dto)
        {
            var payment = await _context.Payments.FindAsync(dto.Id);
            if (payment == null)
            {
                return null;
            }

            var previousInvoiceId = payment.InvoiceId;
            payment.InvoiceId = dto.InvoiceId;
            payment.Method = dto.Method;
            payment.Status = dto.Status;
            payment.TransactionReference = dto.TransactionReference;
            payment.Amount = dto.Amount;
            payment.CurrencyCode = string.IsNullOrWhiteSpace(dto.CurrencyCode) ? payment.CurrencyCode : dto.CurrencyCode;
            payment.ProcessedAt = dto.ProcessedAt;

            await _context.SaveChangesAsync();
            await SyncInvoicePaymentStatusAsync(payment.InvoiceId);
            await SyncReservationPaymentStatusForInvoiceAsync(payment.InvoiceId);
            if (previousInvoiceId != payment.InvoiceId)
            {
                await SyncInvoicePaymentStatusAsync(previousInvoiceId);
                await SyncReservationPaymentStatusForInvoiceAsync(previousInvoiceId);
            }

            await _context.SaveChangesAsync();
            return ToReadDto(payment);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return false;
            }

            var invoiceId = payment.InvoiceId;
            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            await SyncInvoicePaymentStatusAsync(invoiceId);
            await SyncReservationPaymentStatusForInvoiceAsync(invoiceId);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PaymentReadDto?> GetByIdAsync(int id)
        {
            var payment = await _context.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            return payment == null ? null : ToReadDto(payment);
        }

        public async Task<IEnumerable<PaymentReadDto>> GetAllAsync()
        {
            return await _context.Payments.AsNoTracking()
                .Select(p => new PaymentReadDto
                {
                    Id = p.Id,
                    InvoiceId = p.InvoiceId,
                    Method = p.Method,
                    Status = p.Status,
                    TransactionReference = p.TransactionReference,
                    Amount = p.Amount,
                    CurrencyCode = p.CurrencyCode,
                    ProcessedAt = p.ProcessedAt
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentReadDto>> GetByAccountIdAsync(int accountId)
        {
            if (accountId <= 0)
            {
                return Array.Empty<PaymentReadDto>();
            }

            return await _context.Payments
                .AsNoTracking()
                .Join(
                    _context.Invoices.AsNoTracking(),
                    payment => payment.InvoiceId,
                    invoice => invoice.Id,
                    (payment, invoice) => new { payment, invoice })
                .Join(
                    _context.Reservations.AsNoTracking(),
                    joined => joined.invoice.ReservationId,
                    reservation => reservation.ReservationId,
                    (joined, reservation) => new { joined.payment, reservation })
                .Where(x => x.reservation.AccountId == accountId)
                .Select(x => new PaymentReadDto
                {
                    Id = x.payment.Id,
                    InvoiceId = x.payment.InvoiceId,
                    Method = x.payment.Method,
                    Status = x.payment.Status,
                    TransactionReference = x.payment.TransactionReference,
                    Amount = x.payment.Amount,
                    CurrencyCode = x.payment.CurrencyCode,
                    ProcessedAt = x.payment.ProcessedAt
                })
                .ToListAsync();
        }

        public async Task<int> DeleteExpiredPendingPaymentsAsync(TimeSpan? maxAge = null)
        {
            var age = maxAge ?? TimeSpan.FromHours(24);
            var cutoff = DateTime.UtcNow.Subtract(age);

            var stalePayments = await _context.Payments
                .Include(p => p.Invoice)
                .Where(p => p.Status.ToLower() == "pending")
                .Where(p =>
                    (p.ProcessedAt.HasValue && p.ProcessedAt.Value <= cutoff)
                    || (!p.ProcessedAt.HasValue && p.Invoice != null && p.Invoice.IssuedDate <= cutoff))
                .ToListAsync();

            if (stalePayments.Count == 0)
            {
                return 0;
            }

            _context.Payments.RemoveRange(stalePayments);
            await _context.SaveChangesAsync();

            var invoiceIds = stalePayments.Select(p => p.InvoiceId).Distinct().ToList();
            foreach (var invoiceId in invoiceIds)
            {
                await SyncInvoicePaymentStatusAsync(invoiceId);
                await SyncReservationPaymentStatusForInvoiceAsync(invoiceId);
            }

            await _context.SaveChangesAsync();
            return stalePayments.Count;
        }

        public async Task<bool> CompletePendingPaymentByReferenceAsync(string transactionReference)
        {
            if (string.IsNullOrWhiteSpace(transactionReference))
            {
                return false;
            }

            var normalized = transactionReference.Trim();
            var payment = await _context.Payments
                .Include(p => p.Invoice)
                .FirstOrDefaultAsync(p =>
                    p.TransactionReference == normalized
                    && p.Status.ToLower() == "pending");

            if (payment == null)
            {
                return false;
            }

            payment.Status = "Paid";
            payment.ProcessedAt = DateTime.UtcNow;
            if (payment.Invoice != null)
            {
                payment.Invoice.Status = "Paid";
                payment.Invoice.IsPaid = true;
            }

            await _context.SaveChangesAsync();
            await SyncInvoicePaymentStatusAsync(payment.InvoiceId);
            await SyncReservationPaymentStatusForInvoiceAsync(payment.InvoiceId);
            await _context.SaveChangesAsync();
            return true;
        }

        private static PaymentReadDto ToReadDto(Payment payment)
        {
            return new PaymentReadDto
            {
                Id = payment.Id,
                InvoiceId = payment.InvoiceId,
                Method = payment.Method,
                Status = payment.Status,
                TransactionReference = payment.TransactionReference,
                Amount = payment.Amount,
                CurrencyCode = payment.CurrencyCode,
                ProcessedAt = payment.ProcessedAt
            };
        }

        private async Task SyncInvoicePaymentStatusAsync(int invoiceId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);
            if (invoice == null)
            {
                return;
            }

            var paidAmount = invoice.Payments
                .Where(IsPaidStatus)
                .Sum(p => (decimal?)p.Amount) ?? 0;
            var hasPayment = invoice.Payments.Any();

            if (paidAmount > 0 && paidAmount >= invoice.TotalAmount)
            {
                invoice.Status = "Paid";
                invoice.IsPaid = true;
                return;
            }

            if (paidAmount > 0 || hasPayment)
            {
                invoice.Status = "Pending";
                invoice.IsPaid = false;
            }
        }

        private async Task SyncReservationPaymentStatusForInvoiceAsync(int invoiceId)
        {
            var reservationId = await _context.Invoices
                .AsNoTracking()
                .Where(i => i.Id == invoiceId)
                .Select(i => (int?)i.ReservationId)
                .FirstOrDefaultAsync();
            if (!reservationId.HasValue)
            {
                return;
            }

            var reservation = await _context.Reservations
                .Include(r => r.Invoices)
                    .ThenInclude(i => i.Payments)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId.Value);
            if (reservation == null)
            {
                return;
            }

            var invoice = reservation.Invoices?
                .OrderByDescending(i => i.IssuedDate)
                .ThenByDescending(i => i.Id)
                .FirstOrDefault();
            var paidAmount = (invoice?.Payments ?? Enumerable.Empty<Payment>())
                .Where(IsPaidStatus)
                .Sum(p => (decimal?)p.Amount) ?? 0;

            reservation.PaymentStatus = ResolveReservationPaymentStatus(invoice, paidAmount);
        }

        private static string ResolveReservationPaymentStatus(Invoice? invoice, decimal paidAmount)
        {
            if (invoice == null || paidAmount <= 0) return "NotPaid";
            return paidAmount >= invoice.TotalAmount ? "FullyPaid" : "Deposited";
        }

        private static bool IsPaidStatus(Payment payment)
        {
            return payment.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase)
                || payment.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)
                || payment.Status.Equals("Captured", StringComparison.OrdinalIgnoreCase);
        }

        private string ResolveProviderName(string method)
        {
            var normalized = method.Trim();
            if (_paymentOptions.MethodAliases.TryGetValue(normalized, out var providerName)
                && !string.IsNullOrWhiteSpace(providerName))
            {
                return providerName.Trim();
            }

            if (normalized.Equals("PayPal", StringComparison.OrdinalIgnoreCase))
            {
                return "PayPal";
            }

            if (normalized.Equals("Mastercard", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("Visa", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("Card", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("Stripe", StringComparison.OrdinalIgnoreCase))
            {
                return "Stripe";
            }

            if (normalized.Equals("CashApp", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("EcoCash", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("Bank Transfer", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("Manual", StringComparison.OrdinalIgnoreCase))
            {
                return "Manual";
            }

            return normalized;
        }

        private async Task NotifyAdminsAsync(int invoiceId, decimal amount, string currencyCode, string method, string paymentStatus, string txRef)
        {
            var superUsers = await _context.Accounts
                .AsNoTracking()
                .Include(a => a.Role)
                .Where(a => a.IsActive
                    && a.Role != null
                    && (a.Role.Name != null && (a.Role.Name.ToLower() == "super" || a.Role.Name.ToLower() == "admin")))
                .ToListAsync();

            if (superUsers.Count == 0)
            {
                return;
            }

            var title = paymentStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase)
                ? "Payment Received"
                : "Payment Initiated";
            var message = paymentStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase)
                ? $"Invoice {invoiceId} was paid via {method}. Amount: {amount:0.00} {currencyCode.ToUpperInvariant()}. Ref: {txRef}."
                : $"Invoice {invoiceId} payment started via {method}. Amount: {amount:0.00} {currencyCode.ToUpperInvariant()}. Ref: {txRef}.";

            foreach (var user in superUsers)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = user.Id,
                    Title = title,
                    Message = message,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }

        private async Task<ProcessPaymentResultDto?> TryGetIdempotentResponseAsync(ProcessPaymentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.IdempotencyKey))
            {
                return null;
            }

            var endpoint = "payments/process";
            var key = dto.IdempotencyKey.Trim();
            var requestHash = BuildRequestHash(dto);

            var existing = await _context.IdempotencyRecords
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Endpoint == endpoint && r.IdempotencyKey == key);

            if (existing == null)
            {
                return null;
            }

            if (existing.ExpiresAtUtc <= DateTime.UtcNow)
            {
                return null;
            }

            if (!string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Idempotency key was reused with a different payment request.");
            }

            if (string.IsNullOrWhiteSpace(existing.ResponseBody))
            {
                return null;
            }

            return JsonSerializer.Deserialize<ProcessPaymentResultDto>(existing.ResponseBody);
        }

        private async Task SaveIdempotentResponseAsync(ProcessPaymentDto dto, ProcessPaymentResultDto response, int statusCode)
        {
            if (string.IsNullOrWhiteSpace(dto.IdempotencyKey))
            {
                return;
            }

            var endpoint = "payments/process";
            var key = dto.IdempotencyKey.Trim();
            var requestHash = BuildRequestHash(dto);

            var existing = await _context.IdempotencyRecords
                .FirstOrDefaultAsync(r => r.Endpoint == endpoint && r.IdempotencyKey == key);

            if (existing != null && !string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Idempotency key was reused with a different payment request.");
            }

            if (existing == null)
            {
                existing = new IdempotencyRecord
                {
                    Endpoint = endpoint,
                    IdempotencyKey = key,
                    RequestHash = requestHash,
                    CreatedAtUtc = DateTime.UtcNow
                };
                _context.IdempotencyRecords.Add(existing);
            }

            existing.ResponseBody = JsonSerializer.Serialize(response);
            existing.StatusCode = statusCode;
            existing.ExpiresAtUtc = DateTime.UtcNow.AddHours(24);

            await _context.SaveChangesAsync();
        }

        private static string BuildRequestHash(ProcessPaymentDto dto)
        {
            var normalized = string.Join("|",
                dto.InvoiceId,
                dto.Method.Trim().ToLowerInvariant(),
                dto.Amount.ToString("0.########", System.Globalization.CultureInfo.InvariantCulture),
                dto.CurrencyCode.Trim().ToUpperInvariant(),
                dto.ReturnUrl.Trim(),
                dto.CancelUrl.Trim());

            return AuthService.HashToken(normalized);
        }
    }
}
