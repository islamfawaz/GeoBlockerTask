namespace Country.Application._DTOs
{
    public sealed record BlockedCountryDto(
    Guid Id,
    string CountryCode,
    string CountryName,
    DateTime BlockedAt,
    bool IsTemporary,
    DateTime? ExpiresAt,
    bool IsExpired
);
}
