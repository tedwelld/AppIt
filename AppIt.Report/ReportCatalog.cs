namespace AppIt.Report;

/// <summary>
/// Placeholder for RDLC-based financial documents. Wire ReportViewer templates here in a future pass.
/// </summary>
public static class ReportCatalog
{
    public static IReadOnlyList<string> AvailableReports { get; } = new[]
    {
        "ReservationInvoice",
        "TrialBalance",
        "DebtorAging",
        "CashierReconciliation"
    };
}
