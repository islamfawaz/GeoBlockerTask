using Country.Domain.Common;
using Country.Domain.Events;
using Country.Domain.Exceptions;
using Country.Domain.ValueObjects;

namespace Country.Domain.Entities
{
    public sealed class BlockedCountry :AggregateRoot
    {
        public CountryCode CountryCode { get; private set; }
        public string CountryName { get; private set; }
        public DateTime BlockedAt { get; private set; }
        public bool IsTemporary { get; private set; }
        public DateTime? ExpiresAt { get; private set; }

        private BlockedCountry(CountryCode countryCode, string countryName, bool isTemporary, DateTime? expiresAt)
        {
            CountryCode = countryCode;
            CountryName = countryName;
            BlockedAt = DateTime.UtcNow;
            IsTemporary = isTemporary;
            ExpiresAt = expiresAt;
        }
        public static BlockedCountry Block(CountryCode countryCode ,string countryName)
        {
            var blockedCountry = new BlockedCountry(countryCode, countryName, false, null);
            blockedCountry.AddDomainEvent(new CountryBlockedEvent(countryCode.Value, countryName, DateTime.UtcNow, false));
            return blockedCountry;
        }
        public static BlockedCountry BlockTemporarily(CountryCode countryCode, string countryName, int durationMinutes)
        {
            if (durationMinutes < 1 || durationMinutes > 1440)
                throw new DomainException("Duration must be between 1 and 1440 minutes");

            if (string.IsNullOrWhiteSpace(countryName))
                throw new DomainException("Country name cannot be empty");

            var expiresAt = DateTime.UtcNow.AddMinutes(durationMinutes);
            var blockedCountry = new BlockedCountry(countryCode, countryName, true, expiresAt);
            blockedCountry.AddDomainEvent(new CountryBlockedEvent(countryCode.Value, countryName, DateTime.UtcNow, true, expiresAt));
            return blockedCountry;
        }
            public void Unblock()
        {
            AddDomainEvent(new CountryUnblockedEvent(CountryCode.Value, CountryName, DateTime.UtcNow));
        }

        public bool IsExpired() => IsTemporary && ExpiresAt.HasValue && DateTime.UtcNow >= ExpiresAt.Value;
    }

    
}
