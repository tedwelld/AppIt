using AppIt.Core.Interfaces.Services;
using Quartz;

namespace AppIt.Api.Infrastructure.Jobs
{
    public class EndOfDayJob : IJob
    {
        private readonly INightAuditService _nightAudit;
        private readonly IDayEndService _dayEnd;

        public EndOfDayJob(INightAuditService nightAudit, IDayEndService dayEnd)
        {
            _nightAudit = nightAudit;
            _dayEnd = dayEnd;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _nightAudit.ProcessReservationProductsAsync();
            await _dayEnd.RunJournalTransactionsAsync();
        }
    }

    public class FiscalJob : IJob
    {
        private readonly IFiscalService _fiscal;

        public FiscalJob(IFiscalService fiscal) => _fiscal = fiscal;

        public async Task Execute(IJobExecutionContext context)
        {
            await _fiscal.FiscalizePendingInvoicesAsync();
            await _fiscal.FiscalizePendingCreditNotesAsync();
        }
    }

    public class SimunyeJob : IJob
    {
        private readonly ISimunyeService _simunye;

        public SimunyeJob(ISimunyeService simunye) => _simunye = simunye;

        public Task Execute(IJobExecutionContext context) => _simunye.ImportFromEmailAsync();
    }

    public class CurrencyExchangeRatesJob : IJob
    {
        public Task Execute(IJobExecutionContext context) => Task.CompletedTask;
    }

    public class SyncRoomsInventoryJob : IJob
    {
        private readonly IBeds24Service _beds24;

        public SyncRoomsInventoryJob(IBeds24Service beds24) => _beds24 = beds24;

        public Task Execute(IJobExecutionContext context) => _beds24.SyncInventoryAsync();
    }

    public class Beds24ApiCallJob : IJob
    {
        private readonly IBeds24Service _beds24;

        public Beds24ApiCallJob(IBeds24Service beds24) => _beds24 = beds24;

        public Task Execute(IJobExecutionContext context) => _beds24.CallApiAsync();
    }

    public class CacheRefreshJob : IJob
    {
        public Task Execute(IJobExecutionContext context) => Task.CompletedTask;
    }

    public class HConnectSyncJob : IJob
    {
        private readonly IHConnectService _hconnect;

        public HConnectSyncJob(IHConnectService hconnect) => _hconnect = hconnect;

        public Task Execute(IJobExecutionContext context) => _hconnect.ProcessPendingRetriesAsync();
    }
}
