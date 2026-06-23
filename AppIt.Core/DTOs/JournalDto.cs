namespace AppIt.Core.DTOs
{
    public class JournalRunResultDto
    {
        public int EntriesCreated { get; set; }
        public int LinesCreated { get; set; }
        public DateTime ProcessingDate { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class TrialBalanceLineDto
    {
        public int AccountId { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
    }
}
