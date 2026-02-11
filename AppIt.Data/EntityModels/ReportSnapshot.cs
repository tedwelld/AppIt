using System;
using AppIt.Data.EntityModels;

namespace AppIt.Data.Entities
{
    public class ReportSnapshot
    {
        public int Id { get; set; }
        public string ReportKey { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string DataJson { get; set; } = null!;
        public DateTime SnapshotDate { get; set; }

        public int GeneratedByUserId { get; set; }
        public Account? GeneratedByUser { get; set; }
    }
}
