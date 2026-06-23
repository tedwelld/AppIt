using AppIt.Core.DTOs;

namespace AppIt.Core.Interfaces.Services
{
    public interface INightAuditService
    {
        Task<int> ProcessReservationProductsAsync(DateTime? auditDate = null);
    }

    public interface ITrialBalanceService
    {
        Task<IEnumerable<TrialBalanceLineDto>> GetTrialBalanceAsync(DateTime? asOf = null);
    }

    public interface IDebtorReportService
    {
        Task<IEnumerable<DebtorLineDto>> GetAgingAsync(int? companyId = null);
    }

    public interface ICreditMemoService
    {
        Task<IEnumerable<CreditMemoReadDto>> GetAllAsync();
        Task<CreditMemoReadDto> CreateAsync(CreateCreditMemoDto dto);
    }

    public interface IFiscalService
    {
        Task<FiscalStatusDto> GetStatusAsync();
        Task<int> FiscalizePendingInvoicesAsync();
        Task<int> FiscalizePendingCreditNotesAsync();
        Task InitializeDeviceAsync();
    }

    public interface IHConnectService
    {
        Task<IEnumerable<HConnectBookingReadDto>> GetPendingAsync();
        Task<int> ProcessPendingRetriesAsync();
        Task<HConnectBookingReadDto?> QueueReservationAsync(int reservationId);
    }

    public interface IBeds24Service
    {
        Task SyncInventoryAsync();
        Task CallApiAsync();
    }

    public interface ISimunyeService
    {
        Task<int> ImportFromEmailAsync();
    }
}
