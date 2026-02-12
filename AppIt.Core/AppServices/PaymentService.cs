using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using AppIt.Data.Entities;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

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
            var invoice = await _context.Invoices.FindAsync(dto.InvoiceId);
            if (invoice == null)
            {
                return new ProcessPaymentResultDto
                {
                    Success = false,
                    Status = "Failed",
                    Provider = ResolveProviderName(dto.Method),
                    Message = "Invoice not found."
                };
            }

            var amount = dto.Amount > 0 ? dto.Amount : invoice.TotalAmount;
            var currencyCode = string.IsNullOrWhiteSpace(dto.CurrencyCode) ? invoice.CurrencyCode : dto.CurrencyCode;
            var providerName = ResolveProviderName(dto.Method);

            if (!_providerMap.TryGetValue(providerName, out var provider))
            {
                return new ProcessPaymentResultDto
                {
                    Success = false,
                    Status = "Failed",
                    Provider = providerName,
                    Message = $"No configured provider for method '{dto.Method}'."
                };
            }

            var providerRequest = new ProcessPaymentDto
            {
                InvoiceId = dto.InvoiceId,
                Method = dto.Method,
                Amount = amount,
                CurrencyCode = currencyCode,
                ReturnUrl = dto.ReturnUrl,
                CancelUrl = dto.CancelUrl
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
                return new ProcessPaymentResultDto
                {
                    Success = false,
                    Status = "Failed",
                    Provider = activeProvider.Name,
                    Message = providerResult.Message
                };
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

            return new ProcessPaymentResultDto
            {
                Success = true,
                Provider = activeProvider.Name,
                Status = paymentStatus,
                Message = providerResult.Message,
                TransactionReference = providerResult.TransactionReference,
                RedirectUrl = providerResult.RedirectUrl,
                PaymentId = payment.Id
            };
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
    }
}
