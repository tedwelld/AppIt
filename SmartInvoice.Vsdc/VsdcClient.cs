namespace SmartInvoice.Vsdc;

/// <summary>
/// ZRA VSDC HTTP client surface (GoldenDusk SmartInvoice.Vsdc parity stub).
/// Full device protocol wiring activates when FiscalIntegration:ZraEnabled is true.
/// Device/CIS state is stored in AppIt.Data (VsdcDeviceInfo, VsdcCodeEntry).
/// </summary>
public interface IVsdcClient
{
    Task<bool> InitializeAsync(CancellationToken cancellationToken = default);
    Task<VsdcFiscalResult> SubmitInvoiceAsync(VsdcInvoicePayload payload, CancellationToken cancellationToken = default);
}

public record VsdcInvoicePayload(string InvoiceNumber, decimal TotalAmount, string CurrencyCode);

public record VsdcFiscalResult(bool Success, long? ReceiptNo, string? QrCodeUrl, string? ErrorMessage);

public class VsdcClientStub : IVsdcClient
{
    public Task<bool> InitializeAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);

    public Task<VsdcFiscalResult> SubmitInvoiceAsync(VsdcInvoicePayload payload, CancellationToken cancellationToken = default) =>
        Task.FromResult(new VsdcFiscalResult(true, DateTime.UtcNow.Ticks % 1_000_000, null, null));
}
