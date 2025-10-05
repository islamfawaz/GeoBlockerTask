using Country.Domain.Entities;
using Country.Domain.ValueObjects;
using System.Collections.Generic;

namespace Country.Domain.Repositories
{
    public interface ICountryRepository
    {
        Task<BlockedCountry?> GetByCodeAsync(CountryCode countryCode, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<BlockedCountry>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(CountryCode countryCode, CancellationToken cancellationToken = default);
        Task AddAsync(BlockedCountry country, CancellationToken cancellationToken = default);
        Task RemoveAsync(CountryCode countryCode, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<BlockedCountry>> GetExpiredTemporaryBlocksAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<BlockedCountry>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default);
        Task<int> GetTotalCountAsync(string? searchTerm = null, CancellationToken cancellationToken = default);
    }
}

