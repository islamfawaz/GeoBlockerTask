using BuildingBlocks.Messaging.Contracts;
using MassTransit;
using Microsoft.Extensions.Caching.Memory;

namespace IP.API.Consumers
{
    public class CountryUnblockedConsumer : IConsumer<CountryUnblockedIntegrationEvent>
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<CountryUnblockedConsumer> _logger;

        public CountryUnblockedConsumer(IMemoryCache cache, ILogger<CountryUnblockedConsumer> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public Task Consume(ConsumeContext<CountryUnblockedIntegrationEvent> context)
        {
            var evt = context.Message;

            // Remove unblocked country from memory cache
            _cache.Remove(evt.CountryCode);

            _logger.LogInformation("[Consumer] Country unblocked: {CountryCode}", evt.CountryCode);
            return Task.CompletedTask;
        }
    }
}
