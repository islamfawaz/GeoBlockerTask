namespace Log.API.Infrastructure
{
    public interface ILogStore
    {
        void Add(LogEntry entry);
        (IReadOnlyList<LogEntry> items, int total) GetPaged(int page, int pageSize);
    }

    public sealed record LogEntry(string Ip, string CountryCode, DateTime Timestamp, bool IsBlocked, string? UserAgent);
}


