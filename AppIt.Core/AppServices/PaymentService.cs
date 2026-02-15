using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using AppIt.Data.Entities;
using AppIt.Data.EntityModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AppIt.Core.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppItDbContext _context;
        private readonly IReadOnlyDictionary<string, IPaymentProvider> _providerMap;

        public PaymentService(AppItDbContext context, IEnumerable<IPaymentProvider> providers)
        {
            _context = context;
            _providerMap = providers.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
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
            if (!providerResult.Success
                && provider.Name.Equals("Stripe", StringComparison.OrdinalIgnoreCase)
                && providerResult.Message.Contains("not configured", StringComparison.OrdinalIgnoreCase)
                && _providerMap.TryGetValue("Manual", out var manualProvider))
            {
                providerResult = await manualProvider.ProcessAsync(providerRequest);
                activeProvider = manualProvider;
            }

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
                TransactionReference = providerResult.TransactionReference,
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

            payment.InvoiceId = dto.InvoiceId;
            payment.Method = dto.Method;
            payment.Status = dto.Status;
            payment.TransactionReference = dto.TransactionReference;
            payment.Amount = dto.Amount;
            payment.CurrencyCode = string.IsNullOrWhiteSpace(dto.CurrencyCode) ? payment.CurrencyCode : dto.CurrencyCode;
            payment.ProcessedAt = dto.ProcessedAt;

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

            _context.Payments.Remove(payment);
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

        private static string ResolveProviderName(string method)
        {
            if (method.Equals("PayPal", StringComparison.OrdinalIgnoreCase))
            {
                return "PayPal";
            }

            if (method.Equals("Mastercard", StringComparison.OrdinalIgnoreCase)
                || method.Equals("Visa", StringComparison.OrdinalIgnoreCase)
                || method.Equals("Card", StringComparison.OrdinalIgnoreCase)
                || method.Equals("Stripe", StringComparison.OrdinalIgnoreCase))
            {
                return "Stripe";
            }

            if (method.Equals("CashApp", StringComparison.OrdinalIgnoreCase)
                || method.Equals("EcoCash", StringComparison.OrdinalIgnoreCase)
                || method.Equals("Bank Transfer", StringComparison.OrdinalIgnoreCase))
            {
                return "Manual";
            }

            return method;
        }

        private async Task NotifyAdminsAsync(int invoiceId, decimal amount, string currencyCode, string method, string paymentStatus, string txRef)
        {
            var superUsers = await _context.Accounts
                .AsNoTracking()
                .Include(a => a.Role)
                .Where(a => a.IsActive
                    && a.Role != null
                    && (a.Role.Name.ToLower() == "super" || a.Role.Name.ToLower() == "admin"))
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
