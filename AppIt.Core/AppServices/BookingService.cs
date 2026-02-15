using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;

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
                var reservation = await _reservationService.CreateAsync(request.Reservation);

                var invoiceRequest = new CreateInvoiceDto
                {
                    ReservationId = reservation.ReservationId,
                    TotalAmount = request.Invoice.TotalAmount <= 0 ? reservation.TotalAmount : request.Invoice.TotalAmount,
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
                    Voucher = voucher
                };
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
    }
}
