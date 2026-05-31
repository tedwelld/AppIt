using AppIt.Core.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AppIt.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/frontend")]
    public class FrontendMetadataController : ControllerBase
    {
        private readonly PaymentProviderOptions _paymentOptions;

        public FrontendMetadataController(IOptions<PaymentProviderOptions> paymentOptions)
        {
            _paymentOptions = paymentOptions.Value;
        }

        [HttpGet("payment-methods")]
        public IActionResult GetPaymentMethods()
        {
            var methods = _paymentOptions.MethodAliases
                .Keys
                .Where(method => !string.IsNullOrWhiteSpace(method))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(method => method)
                .Select(method => new
                {
                    label = method,
                    value = method,
                    provider = _paymentOptions.MethodAliases[method]
                })
                .ToList();

            return Ok(methods);
        }

        [HttpGet("service-types")]
        public IActionResult GetServiceTypes()
        {
            return Ok(new[]
            {
                new { label = "Lodging", value = "Product", endpoint = "/api/products" },
                new { label = "Accommodations", value = "Accommodation", endpoint = "/api/accommodations" },
                new { label = "Activities", value = "Activity", endpoint = "/api/activities" },
                new { label = "Transfers", value = "Transfer", endpoint = "/api/transfers" },
                new { label = "Tours", value = "Tour", endpoint = "/api/tours" }
            });
        }

        [HttpGet("report-catalog")]
        public IActionResult GetReportCatalog()
        {
            return Ok(new[]
            {
                new { label = "Reservation Pipeline", path = "/api/reservations", key = "reservation-pipeline", title = "Reservation Pipeline Report", group = "Reservations", description = "All captured reservations, client contacts, totals, vouchers, and current statuses." },
                new { label = "Pending Bookings", path = "/api/reservations", key = "pending-bookings", title = "Pending Bookings Report", group = "Reservations", description = "Booking report filtered in the report studio to pending reservations." },
                new { label = "Completed Bookings", path = "/api/reservations", key = "completed-bookings", title = "Completed Bookings Report", group = "Reservations", description = "Booking report filtered in the report studio to completed or closed reservations." },
                new { label = "Open Bookings", path = "/api/reservations", key = "open-bookings", title = "Open Bookings Report", group = "Reservations", description = "Booking report filtered in the report studio to open reservations." },
                new { label = "Cancelled Bookings", path = "/api/reservations", key = "cancelled-bookings", title = "Cancelled Bookings Report", group = "Reservations", description = "Booking report filtered in the report studio to cancelled reservations." },
                new { label = "Booking Utilization", path = "/api/reservations", key = "booking-utilization", title = "Booking Utilization Report", group = "Reservations", description = "Booking activity and reservation value from AppIt reservation records." },
                new { label = "Voucher Summary", path = "/api/vouchers", key = "voucher-summary", title = "Voucher Summary Report", group = "Reservations", description = "Generated vouchers linked to reservations." },
                new { label = "Occupancy Source Data", path = "/api/reservations", key = "occupancy-source-data", title = "Occupancy Source Data Report", group = "Reservations", description = "Reservation rows used by occupancy and availability views." },
                new { label = "Cashier Payments", path = "/api/payments", key = "cashier-payments", title = "Cashier Payments Report", group = "Cashier", description = "Payment methods, statuses, references, and processed amounts." },
                new { label = "Exchange Rate Source", path = "/api/cashier/exchange-rates", key = "exchange-rate-source", title = "Exchange Rate Source Report", group = "Cashier", description = "Currency rows currently visible from AppIt payment and reservation activity." },
                new { label = "Bank Note Summary", path = "/api/cashier/bank-note-details", key = "bank-note-summary", title = "Bank Note Summary Report", group = "Cashier", description = "Cashier totals grouped by currency, method, and payment status." },
                new { label = "Deposit Report", path = "/api/payments", key = "deposit-report", title = "Deposit Report", group = "Cashier", description = "Deposit and manual payment review using AppIt payment records." },
                new { label = "Proof Of Payment", path = "/api/payments", key = "proof-of-payment", title = "Proof Of Payment Report", group = "Accounts", description = "Payment proof/reconciliation view from transaction references." },
                new { label = "Invoice Financials", path = "/api/invoices", key = "invoice-financials", title = "Invoice Financials Report", group = "Accounts", description = "Invoice totals, issue dates, reservation links, and payment status." },
                new { label = "Credit Notes And Refund Source", path = "/api/payments", key = "credit-notes-refund-source", title = "Credit Notes And Refund Source Report", group = "Accounts", description = "Payment source rows for future credit-note/refund workflows." },
                new { label = "Executive Stats Source", path = "/api/admin/stats", key = "executive-stats-source", title = "Executive Stats Source Report", group = "Statistics", description = "Admin statistics returned by the backend summary endpoint." },
                new { label = "Accounts Access Matrix", path = "/api/accounts", key = "accounts-access-matrix", title = "Accounts Access Matrix Report", group = "Administration", description = "User accounts, roles, activity status, and access metadata." },
                new { label = "User Activity Log", path = "/api/audit-logs", key = "user-activity-log", title = "User Activity Log Report", group = "Administration", description = "Audited actions captured by the AppIt audit log." },
                new { label = "Operations Check In", path = "/api/reservations", key = "operations-check-in", title = "Operations Check In Report", group = "Operations", description = "Reservation rows used by operations check-in views." },
                new { label = "Operations Voucher Sync", path = "/api/vouchers", key = "operations-voucher-sync", title = "Operations Voucher Sync Report", group = "Operations", description = "Voucher rows used by operations integration views." }
            });
        }
    }
}
