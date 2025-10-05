using Country.Domain.Entities;
using Country.Domain.Repositories;
using Country.Domain.ValueObjects;
using System.Collections.Concurrent;

namespace Country.Infrastructure.Repositories
{
    public class CountryRepository : ICountryRepository
    {
        private readonly ConcurrentDictionary<string, BlockedCountry> _countries = new();
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        // Additional indexes for faster searches
        private readonly ConcurrentDictionary<string, HashSet<string>> _nameIndex = new();
        private volatile int _cachedTotalCount = 0;
        private DateTime _lastCacheUpdate = DateTime.UtcNow;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromSeconds(30);

        public async Task<BlockedCountry?> GetByCodeAsync(
            CountryCode countryCode,
            CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            _countries.TryGetValue(countryCode.Value, out var country);
            return country;
        }

        public async Task<IReadOnlyList<BlockedCountry>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            return _countries.Values.OrderByDescending(c => c.BlockedAt).ToList().AsReadOnly();
        }

        public async Task<bool> ExistsAsync(
            CountryCode countryCode,
            CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            return _countries.ContainsKey(countryCode.Value);
        }

        public async Task AddAsync(
            BlockedCountry country,
            CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                if (_countries.TryAdd(country.CountryCode.Value, country))
                {
                    // Update name index
                    UpdateNameIndex(country.CountryName, country.CountryCode.Value, isAdd: true);
                    InvalidateCache();
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task RemoveAsync(
            CountryCode countryCode,
            CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                if (_countries.TryRemove(countryCode.Value, out var removedCountry))
                {
                    // Update name index
                    UpdateNameIndex(removedCountry.CountryName, countryCode.Value, isAdd: false);
                    InvalidateCache();
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<IReadOnlyList<BlockedCountry>> GetExpiredTemporaryBlocksAsync(
            CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;

            var now = DateTime.UtcNow;
            var expired = _countries.Values
                .Where(c => c.IsTemporary &&
                           c.ExpiresAt.HasValue &&
                           now >= c.ExpiresAt.Value)
                .ToList();

            return expired.AsReadOnly();
        }

        public async Task<IReadOnlyList<BlockedCountry>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;

            IEnumerable<BlockedCountry> query = _countries.Values;

            // Use index for search if available
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = SearchCountries(searchTerm);
            }

            var pagedCountries = query
                .OrderByDescending(c => c.BlockedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return pagedCountries.AsReadOnly();
        }

        public async Task<int> GetTotalCountAsync(
            string? searchTerm = null,
            CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;

            // Use cache for total count without search
            if (string.IsNullOrWhiteSpace(searchTerm) && IsCacheValid())
            {
                return _cachedTotalCount;
            }

            IEnumerable<BlockedCountry> query = _countries.Values;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = SearchCountries(searchTerm);
            }

            var count = query.Count();

            // Update cache if no search term
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                _cachedTotalCount = count;
                _lastCacheUpdate = DateTime.UtcNow;
            }

            return count;
        }

        // Private helper methods

        private void UpdateNameIndex(string countryName, string countryCode, bool isAdd)
        {
            if (string.IsNullOrWhiteSpace(countryName))
                return;

            var normalizedName = countryName.ToUpperInvariant();
            var words = normalizedName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                if (isAdd)
                {
                    _nameIndex.AddOrUpdate(
                        word,
                        new HashSet<string> { countryCode },
                        (key, existing) =>
                        {
                            existing.Add(countryCode);
                            return existing;
                        });
                }
                else
                {
                    if (_nameIndex.TryGetValue(word, out var codes))
                    {
                        codes.Remove(countryCode);
                        if (codes.Count == 0)
                        {
                            _nameIndex.TryRemove(word, out _);
                        }
                    }
                }
            }
        }

        private IEnumerable<BlockedCountry> SearchCountries(string searchTerm)
        {
            var searchTermUpper = searchTerm.ToUpperInvariant();

            // Direct code match
            var codeMatches = _countries.Values
                .Where(c => c.CountryCode.Value.Contains(searchTermUpper, StringComparison.OrdinalIgnoreCase));

            // Name match using index
            var nameMatches = _countries.Values
                .Where(c => c.CountryName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));

            return codeMatches.Union(nameMatches);
        }

        private bool IsCacheValid()
        {
            return (DateTime.UtcNow - _lastCacheUpdate) < _cacheExpiration;
        }

        private void InvalidateCache()
        {
            _lastCacheUpdate = DateTime.MinValue;
        }

        public void Clear()
        {
            _countries.Clear();
            _nameIndex.Clear();
            InvalidateCache();
        }
    }
}

    
