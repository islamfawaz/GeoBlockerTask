using Country.Domain.Common;
using BuildingBlocks.Messaging.Contracts;

namespace Country.Domain.Events
{
    public sealed record CountryUnblockedEvent(
        string CountryCode,
        string CountryName,
        DateTime UnblockedAt) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public CountryUnblockedIntegrationEvent ToIntegrationEvent() =>
            new CountryUnblockedIntegrationEvent(CountryCode, CountryName, UnblockedAt);
    }
}
