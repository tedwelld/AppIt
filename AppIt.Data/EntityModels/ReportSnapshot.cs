using System;
using AppIt.Data;

namespace AppIt.Data.Entities
{
    public class ReportSnapshot
    {
        public int Id { get; set; }

        public string ReportKey { get; set; } = null!;
        // e.g. "DailySales", "CustomerSummary", "RevenueByProduct"

        public string Title { get; set; } = null!;

        public string DataJson { get; set; } = null!;
        // Serialized snapshot payload

        public DateTime SnapshotDate { get; set; }

        public int GeneratedByUserId { get; set; }
    }
}
