using BuildingBlocks.Messaging.Contracts;
using Log.API.Infrastructure;
using MassTransit;

namespace Log.API.Consumers
{
    public sealed class BlockedIpAttemptConsumer : IConsumer<BlockedIpAttemptIntegrationEvent>
    {
        private readonly ILogStore _store;
        public BlockedIpAttemptConsumer(ILogStore store) => _store = store;
        public Task Consume(ConsumeContext<BlockedIpAttemptIntegrationEvent> context)
        {
            var e = context.Message;
            _store.Add(new LogEntry(e.Ip, e.CountryCode, e.AttemptedAt, e.IsBlocked, e.UserAgent));
            return Task.CompletedTask;
        }
    }
}


