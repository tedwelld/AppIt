using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Core.Services
{
    public class BookingService : IBookingService
    {
        private readonly AppItDbContext _context;
        private readonly IReservationService _reservationService;
        private readonly IInvoiceService _invoiceService;
        private readonly IPaymentService _paymentService;
        private readonly IVoucherService _voucherService;

        public BookingService(
            AppItDbContext context,
            IReservationService reservationService,
            IInvoiceService invoiceService,
            IPaymentService paymentService,
            IVoucherService voucherService)
        {
            _context = context;
            _reservationService = reservationService;
            _invoiceService = invoiceService;
            _paymentService = paymentService;
            _voucherService = voucherService;
        }

        public async Task<BookingCheckoutResultDto> CheckoutAsync(BookingCheckoutRequestDto request, int accountId)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                request.Reservation.AccountId = accountId;
                var customer = await ResolveCustomerAsync(request, accountId);
                request.Reservation.CustomerId = customer.Id;
                request.Reservation.CustomerEmail = customer.Email;
                request.Reservation.AgencyId = request.TripAccountId ?? customer.AgentCompanyId;

                var serviceItems = NormalizeServiceItems(request.ServiceItems);
                var totalAmount = serviceItems.Sum(item => item.TotalPrice);
                if (totalAmount <= 0)
                {
                    throw new InvalidOperationException("At least one service item with a valid total is required.");
                }

                request.Reservation.TotalAmount = totalAmount;
                request.Reservation.Currency = serviceItems.First().Currency;
                var reservation = await _reservationService.CreateAsync(request.Reservation);
                var savedItems = serviceItems.Select(item => new ReservationServiceItem
                {
                    ReservationId = reservation.ReservationId,
                    ServiceType = item.ServiceType,
                    ServiceId = item.ServiceId,
                    ServiceName = item.ServiceName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice,
                    Currency = item.Currency
                }).ToList();
                _context.ReservationServiceItems.AddRange(savedItems);
                await _context.SaveChangesAsync();

                var invoiceRequest = new CreateInvoiceDto
                {
                    ReservationId = reservation.ReservationId,
                    TotalAmount = totalAmount,
                    Currency = string.IsNullOrWhiteSpace(request.Invoice.Currency) ? reservation.Currency : request.Invoice.Currency,
                    Status = string.IsNullOrWhiteSpace(request.Invoice.Status) ? "Pending" : request.Invoice.Status
                };

                var invoice = await _invoiceService.CreateAsync(invoiceRequest);

                var paymentRequest = new ProcessPaymentDto
                {
                    InvoiceId = invoice.Id,
                    Method = request.Payment.Method,
                    Amount = request.Payment.Amount <= 0 ? invoice.TotalAmount : request.Payment.Amount,
                    CurrencyCode = string.IsNullOrWhiteSpace(request.Payment.CurrencyCode) ? invoice.Currency : request.Payment.CurrencyCode,
                    ReturnUrl = request.Payment.ReturnUrl,
                    CancelUrl = request.Payment.CancelUrl,
                    IdempotencyKey = request.Payment.IdempotencyKey
                };

                var payment = await _paymentService.ProcessAsync(paymentRequest);
                if (!payment.Success)
                {
                    throw new InvalidOperationException(payment.Message);
                }

                VoucherReadDto? voucher = null;
                var voucherRequest = request.Voucher ?? new CreateVoucherDto
                {
                    Code = reservation.VoucherCode,
                    Reference = reservation.Reference,
                    Type = "Reservation",
                    ReservationId = reservation.ReservationId
                };

                if (string.IsNullOrWhiteSpace(voucherRequest.Code))
                {
                    voucherRequest.Code = reservation.VoucherCode;
                }

                if (string.IsNullOrWhiteSpace(voucherRequest.Reference))
                {
                    voucherRequest.Reference = reservation.Reference;
                }

                voucherRequest.ReservationId = reservation.ReservationId;
                voucher = await _voucherService.CreateAsync(voucherRequest);

                await tx.CommitAsync();
                return new BookingCheckoutResultDto
                {
                    Reservation = reservation,
                    Invoice = invoice,
                    Payment = payment,
                    Voucher = voucher,
                    Customer = ToCustomerReadDto(customer),
                    ServiceItems = savedItems.Select(ToServiceItemDto).ToList()
                };
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        private async Task<Customer> ResolveCustomerAsync(BookingCheckoutRequestDto request, int accountId)
        {
            if (request.CustomerId.HasValue && request.CustomerId.Value > 0)
            {
                var existing = await _context.Customers.FirstOrDefaultAsync(c => c.Id == request.CustomerId.Value);
                if (existing == null)
                {
                    throw new InvalidOperationException("Selected customer was not found.");
                }

                if (request.TripAccountId.HasValue)
                {
                    existing.AgentCompanyId = request.TripAccountId;
                    await _context.SaveChangesAsync();
                }

                return existing;
            }

            var dto = request.Customer ?? throw new InvalidOperationException("Customer details are required.");
            var hasName = !string.IsNullOrWhiteSpace(dto.FirstName) || !string.IsNullOrWhiteSpace(dto.Surname) || !string.IsNullOrWhiteSpace(dto.ProxyName);
            var hasContact = !string.IsNullOrWhiteSpace(dto.Email) || !string.IsNullOrWhiteSpace(dto.PhoneNumber);
            if (!hasName)
            {
                throw new InvalidOperationException("Client first name, surname, or contact name is required.");
            }

            if (!hasContact)
            {
                throw new InvalidOperationException("Client email or phone is required.");
            }

            var customer = new Customer
            {
                Title = dto.Title,
                FirstName = dto.FirstName,
                Surname = dto.Surname,
                IdType = dto.IdType,
                Nationality = dto.Nationality,
                Dob = dto.Dob,
                Profession = dto.Profession,
                ProxyName = dto.ProxyName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Image = dto.Image,
                TaxCategory = dto.TaxCategory,
                Region = dto.Region,
                DurationOfStayDays = dto.DurationOfStayDays,
                LastSavedBy = accountId,
                DateUpdated = DateTime.UtcNow,
                Notes = dto.Notes,
                AgentCompanyId = request.TripAccountId ?? dto.AgentCompanyId
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        private static List<BookingServiceItemDto> NormalizeServiceItems(IEnumerable<BookingServiceItemDto>? items)
        {
            var normalized = (items ?? Enumerable.Empty<BookingServiceItemDto>())
                .Select(item =>
                {
                    var quantity = item.Quantity < 1 ? 1 : item.Quantity;
                    var unitPrice = item.UnitPrice;
                    var totalPrice = quantity * unitPrice;
                    return new BookingServiceItemDto
                    {
                        ServiceType = NormalizeServiceType(item.ServiceType),
                        ServiceId = item.ServiceId,
                        ServiceName = string.IsNullOrWhiteSpace(item.ServiceName) ? "Service" : item.ServiceName.Trim(),
                        Quantity = quantity,
                        UnitPrice = unitPrice,
                        TotalPrice = totalPrice,
                        Currency = string.IsNullOrWhiteSpace(item.Currency) ? "USD" : item.Currency.Trim().ToUpperInvariant()
                    };
                })
                .ToList();

            if (normalized.Count == 0)
            {
                throw new InvalidOperationException("At least one service item is required.");
            }

            if (normalized.Any(item => item.ServiceId <= 0 || item.UnitPrice <= 0 || item.TotalPrice <= 0))
            {
                throw new InvalidOperationException("Each service item must have a service, quantity, and price.");
            }

            if (normalized.Select(item => item.Currency).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1)
            {
                throw new InvalidOperationException("All service items must use the same currency.");
            }

            return normalized;
        }

        private static string NormalizeServiceType(string? serviceType)
        {
            var value = (serviceType ?? string.Empty).Trim();
            return value.Equals("Accommodation", StringComparison.OrdinalIgnoreCase)
                ? "Accommodation"
                : value.Equals("Activity", StringComparison.OrdinalIgnoreCase)
                    ? "Activity"
                    : "Product";
        }

        private static BookingServiceItemDto ToServiceItemDto(ReservationServiceItem item)
        {
            return new BookingServiceItemDto
            {
                Id = item.Id,
                ServiceType = item.ServiceType,
                ServiceId = item.ServiceId,
                ServiceName = item.ServiceName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice,
                Currency = item.Currency
            };
        }

        private static CustomerReadDto ToCustomerReadDto(Customer customer)
        {
            return new CustomerReadDto
            {
                Id = customer.Id,
                Title = customer.Title,
                FirstName = customer.FirstName,
                Surname = customer.Surname,
                IdType = customer.IdType,
                Nationality = customer.Nationality,
                Dob = customer.Dob,
                Profession = customer.Profession,
                ProxyName = customer.ProxyName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                Image = customer.Image,
                TaxCategory = customer.TaxCategory,
                Region = customer.Region,
                DurationOfStayDays = customer.DurationOfStayDays,
                LastSavedBy = customer.LastSavedBy,
                DateUpdated = customer.DateUpdated,
                Notes = customer.Notes,
                CustomerTypeId = customer.CustomerType?.Id,
                AgentCompanyId = customer.AgentCompanyId,
                ReservationIds = customer.Reservations.Select(r => r.ReservationId).ToList()
            };
        }
    }
}
