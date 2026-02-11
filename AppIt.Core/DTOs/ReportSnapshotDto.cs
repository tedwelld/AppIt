namespace AppIt.Core.DTOs
{
    public class ReportSnapshotDto
    {
        public int Id { get; set; }
        public string ReportKey { get; set; } = null!;
        public string Title { get; set; } = null!;
        public DateTime SnapshotDate { get; set; }
    }

    public class ReportSnapshotDetailDto
    {
        public int Id { get; set; }
        public string ReportKey { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string DataJson { get; set; } = null!;
        public DateTime SnapshotDate { get; set; }
        public int GeneratedByUserId { get; set; }
    }

    public class CreateReportSnapshotDto
    {
        public string ReportKey { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string DataJson { get; set; } = null!;
        public DateTime SnapshotDate { get; set; }
        public int GeneratedByUserId { get; set; }
    }
}
