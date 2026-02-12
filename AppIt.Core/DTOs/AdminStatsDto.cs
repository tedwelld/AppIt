using System.Collections.Generic;

namespace AppIt.Core.DTOs
{
    public class AdminStatsDto
    {
        public string Range { get; set; } = "weekly";
        public int TotalSales { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalEarnings { get; set; }
        public int TotalCustomers { get; set; }
        public List<int> Trend { get; set; } = new();
    }
}
