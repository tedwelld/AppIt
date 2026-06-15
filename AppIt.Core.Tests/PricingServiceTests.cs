using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using AppIt.Core.Services;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AppIt.Core.Tests;

public class PricingServiceTests
{
    [Fact]
    public async Task ResolveUnitPriceAsync_UsesDirectServicePrice()
    {
        await using var db = CreateContext();
        db.ServicePrices.Add(new ServicePrice
        {
            ServiceType = "Product",
            ServiceId = 1,
            CurrencyCode = "USD",
            UnitPrice = 42m,
            IsActive = true
        });
        await db.SaveChangesAsync();

        var service = new PricingService(db, new StubExchangeRateService());
        var price = await service.ResolveUnitPriceAsync("Product", 1, "USD");

        Assert.Equal(42m, price);
    }

    [Fact]
    public async Task GetEffectiveRateAsync_ThrowsWhenRateMissing()
    {
        await using var db = CreateContext();
        var service = new PricingService(db, new StubExchangeRateService());

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetEffectiveRateAsync("ZAR"));
    }

    private static AppItDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppItDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppItDbContext(options);
    }

    private sealed class StubExchangeRateService : IExchangeRateService
    {
        public Task<ExchangeRateDto> CreateExchangeRateAsync(CreateExchangeRateDto dto) => throw new NotImplementedException();
        public Task<ExchangeRateDto?> GetExchangeRateByIdAsync(int id) => Task.FromResult<ExchangeRateDto?>(null);
        public Task<IEnumerable<ExchangeRateDto>> GetAllExchangeRatesAsync() => Task.FromResult<IEnumerable<ExchangeRateDto>>(Array.Empty<ExchangeRateDto>());
        public Task<IEnumerable<ExchangeRateDto>> GetExchangeRatesByDateAsync(DateTime date) => Task.FromResult<IEnumerable<ExchangeRateDto>>(Array.Empty<ExchangeRateDto>());
        public Task<IEnumerable<ExchangeRateDto>> GetEffectiveRatesAsync(DateTime date) => Task.FromResult<IEnumerable<ExchangeRateDto>>(Array.Empty<ExchangeRateDto>());
        public Task<ExchangeRateDto?> GetEffectiveRateAsync(string currencyCode, DateTime date) => Task.FromResult<ExchangeRateDto?>(null);
        public Task<ExchangeRateDto?> UpdateExchangeRateAsync(int id, UpdateExchangeRateDto dto) => Task.FromResult<ExchangeRateDto?>(null);
        public Task<bool> DeleteExchangeRateAsync(int id) => Task.FromResult(false);
    }
}
