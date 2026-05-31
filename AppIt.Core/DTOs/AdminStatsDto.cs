using System.Collections.Generic;

namespace AppIt.Core.DTOs
{
    public class AdminStatsDto
    {
        public string Range { get; set; } = "weekly";
        public int TotalAccounts { get; set; }
        public int TotalReservations { get; set; }
        public int TotalInvoices { get; set; }
        public int TotalPayments { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalSales { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalEarnings { get; set; }
        public int TotalCustomers { get; set; }
        public int PendingPayments { get; set; }
        public int ActiveVouchers { get; set; }
        public List<int> Trend { get; set; } = new();
    }
}
