using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AppIt.Core.AppServices.Fiscalization
{
    public class FiscalService : IFiscalService
    {
        private readonly AppItDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<FiscalService> _logger;

        public FiscalService(AppItDbContext context, IConfiguration config, ILogger<FiscalService> logger)
        {
            _context = context;
            _config = config;
            _logger = logger;
        }

        private bool ZraEnabled => _config.GetValue<bool>("FiscalIntegration:ZraEnabled");

        public async Task<FiscalStatusDto> GetStatusAsync()
        {
            var device = await _context.VsdcDeviceInfos.AsNoTracking().FirstOrDefaultAsync();
            return new FiscalStatusDto
            {
                ZraEnabled = ZraEnabled,
                DeviceInitialized = device?.IsInitialized ?? false,
                PendingInvoices = await _context.Invoices.CountAsync(i => !i.IsFiscalized && i.Status != "Cancelled"),
                PendingCreditNotes = await _context.CreditNotes.CountAsync(c => c.Status == "Pending")
            };
        }

        public async Task InitializeDeviceAsync()
        {
            var device = await _context.VsdcDeviceInfos.FirstOrDefaultAsync();
            if (device == null)
            {
                device = new VsdcDeviceInfo { DeviceId = _config["FiscalIntegration:DeviceId"] ?? "DEV" };
                _context.VsdcDeviceInfos.Add(device);
            }
            device.IsInitialized = true;
            device.InitializedAt = DateTime.UtcNow;
            device.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _logger.LogInformation("VSDC device initialized (ZraEnabled={Enabled})", ZraEnabled);
        }

        public async Task<int> FiscalizePendingInvoicesAsync()
        {
            var pending = await _context.Invoices.Where(i => !i.IsFiscalized).Take(50).ToListAsync();
            foreach (var inv in pending)
            {
                inv.IsFiscalized = true;
                inv.DateFiscalized = DateTime.UtcNow;
                inv.FiscalReceiptNo = DateTime.UtcNow.Ticks % 1_000_000;
                inv.FiscalCisInvoiceNo = $"CIS-{inv.Id:D6}";
                if (ZraEnabled)
                {
                    _logger.LogInformation("Would submit invoice {Id} to ZRA VSDC", inv.Id);
                }
            }
            await _context.SaveChangesAsync();
            return pending.Count;
        }

        public async Task<int> FiscalizePendingCreditNotesAsync()
        {
            var pending = await _context.CreditNotes.Where(c => c.Status == "Pending").Take(50).ToListAsync();
            foreach (var cn in pending)
            {
                cn.Status = "Fiscalized";
            }
            await _context.SaveChangesAsync();
            return pending.Count;
        }
    }
}
