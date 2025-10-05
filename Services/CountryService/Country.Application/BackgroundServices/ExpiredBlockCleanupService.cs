using Country.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Country.Application.BackgroundServices
{
    public sealed class ExpiredBlockCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExpiredBlockCleanupService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);

        public ExpiredBlockCleanupService(
            IServiceProvider serviceProvider,
            ILogger<ExpiredBlockCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Expired Block Cleanup Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredBlocksAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up expired blocks");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Expired Block Cleanup Service stopped");
        }

        private async Task CleanupExpiredBlocksAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ICountryRepository>();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            var expiredBlocks = await repository.GetExpiredTemporaryBlocksAsync(cancellationToken);

            if (!expiredBlocks.Any())
                return;

            _logger.LogInformation("Found {Count} expired temporary blocks to clean up", expiredBlocks.Count);

            foreach (var block in expiredBlocks)
            {
                try
                {
                    block.Unblock();
                    await repository.RemoveAsync(block.CountryCode, cancellationToken);

                    // Publish domain events via MediatR
                    foreach (var domainEvent in block.DomainEvents)
                    {
                        await mediator.Publish(domainEvent, cancellationToken);
                    }
                    block.ClearDomainEvents();

                    _logger.LogInformation(
                        "Removed expired temporary block for country {CountryCode}",
                        block.CountryCode.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error removing expired block for country {CountryCode}",
                        block.CountryCode.Value);
                }
            }
        }
    }

}
