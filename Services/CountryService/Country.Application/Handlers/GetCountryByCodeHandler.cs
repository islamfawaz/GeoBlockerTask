using BuildingBlocks.CQRS;
using Country.Application._DTOs;
using Country.Application.Queries;
using Country.Domain.Repositories;
using Country.Domain.ValueObjects;

namespace Country.Application.Handlers
{
    public sealed class GetCountryByCodeHandler
         : IQueryHandler<GetCountryByCodeQuery, BlockedCountryDto>
    {
        private readonly ICountryRepository _repository;

        public GetCountryByCodeHandler(ICountryRepository repository)
        {
            _repository = repository;
        }

        public async Task<BlockedCountryDto> Handle(
            GetCountryByCodeQuery request,
            CancellationToken cancellationToken)
        {
            var countryCode = CountryCode.Create(request.CountryCode);
            var country = await _repository.GetByCodeAsync(countryCode, cancellationToken);

            if (country == null)
                throw new InvalidOperationException($"Country with code '{request.CountryCode}' not found.");

            return new BlockedCountryDto(
                country.Id,
                country.CountryCode.Value,
                country.CountryName,
                country.BlockedAt,
                country.IsTemporary,
                country.ExpiresAt,
                country.IsExpired()
            );
        }
    }
}
