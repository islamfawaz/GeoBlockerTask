namespace Log.API.Infrastructure
{
    public sealed class InMemoryLogStore : ILogStore
    {
        private readonly List<LogEntry> _entries = new();
        private readonly object _lock = new();
        public void Add(LogEntry entry)
        {
            lock (_lock)
            {
                _entries.Add(entry);
            }
        }
        public (IReadOnlyList<LogEntry> items, int total) GetPaged(int page, int pageSize)
        {
            lock (_lock)
            {
                var total = _entries.Count;
                var items = _entries
                    .OrderByDescending(e => e.Timestamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                return (items, total);
            }
        }
    }
}


