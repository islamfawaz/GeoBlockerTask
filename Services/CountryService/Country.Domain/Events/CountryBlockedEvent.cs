using Country.Domain.Common;
using BuildingBlocks.Messaging.Contracts;

namespace Country.Domain.Events
{
    public sealed record CountryBlockedEvent(
     string CountryCode,
     string CountryName,
     DateTime BlockedAt,
     bool IsTemporary,
     DateTime? ExpiresAt = null) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public CountryBlockedIntegrationEvent ToIntegrationEvent() =>
            new CountryBlockedIntegrationEvent(CountryCode, CountryName, BlockedAt, IsTemporary, ExpiresAt);
    }
}
