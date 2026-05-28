using AppIt.Core.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AppIt.Api.Infrastructure
{
    public class PendingPaymentCleanupHostedService : BackgroundService
    {
        private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(24);
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PendingPaymentCleanupHostedService> _logger;

        public PendingPaymentCleanupHostedService(
            IServiceScopeFactory scopeFactory,
            ILogger<PendingPaymentCleanupHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await CleanupAsync(stoppingToken);

            using var timer = new PeriodicTimer(CleanupInterval);
            while (!stoppingToken.IsCancellationRequested
                && await timer.WaitForNextTickAsync(stoppingToken))
            {
                await CleanupAsync(stoppingToken);
            }
        }

        private async Task CleanupAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
                var deletedCount = await paymentService.DeleteExpiredPendingPaymentsAsync(CleanupInterval);

                if (deletedCount > 0)
                {
                    _logger.LogInformation("Pending payment cleanup deleted {DeletedCount} expired records.", deletedCount);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // graceful shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to run pending payment cleanup task.");
            }
        }
    }
}
