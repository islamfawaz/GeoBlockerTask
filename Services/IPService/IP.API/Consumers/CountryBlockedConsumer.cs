using BuildingBlocks.Messaging.Contracts;
using MassTransit;
using Microsoft.Extensions.Caching.Memory;

namespace IP.API.Consumers
{
    public class CountryBlockedConsumer : IConsumer<CountryBlockedIntegrationEvent>
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<CountryBlockedConsumer> _logger;

        public CountryBlockedConsumer(IMemoryCache cache, ILogger<CountryBlockedConsumer> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public Task Consume(ConsumeContext<CountryBlockedIntegrationEvent> context)
        {
            var evt = context.Message;


            // Store blocked country in memory cache
            _cache.Set(evt.CountryCode, evt, new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = evt.IsTemporary && evt.ExpiresAt.HasValue
                    ? evt.ExpiresAt
                    : DateTimeOffset.MaxValue
            });

            _logger.LogInformation("[Consumer] Country blocked: {CountryCode}", evt.CountryCode);
            return Task.CompletedTask;
        }
    }
}
