namespace BuildingBlocks.Messaging.Contracts
{
    // Integration events for cross-service communication
    public record CountryBlockedIntegrationEvent(string CountryCode, string CountryName, DateTime BlockedAt, bool IsTemporary, DateTime? ExpiresAt);
    public record CountryUnblockedIntegrationEvent(string CountryCode, string CountryName, DateTime UnblockedAt);

    public record BlockedIpAttemptIntegrationEvent(
        string Ip,
        string CountryCode,
        DateTime AttemptedAt,
        string? UserAgent,
        bool IsBlocked
    );
}


