using BuildingBlocks.CQRS;
using Country.Application._DTOs;
using Country.Application.Queries;
using Country.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Country.Application.Handlers
{

    public sealed class GetBlockedCountriesHandler
        : IQueryHandler<GetBlockedCountriesQuery, PagedResult<BlockedCountryDto>>
    {
        private readonly ICountryRepository _repository;
        private readonly ILogger<GetBlockedCountriesHandler> _logger;

        public GetBlockedCountriesHandler(
            ICountryRepository repository,
            ILogger<GetBlockedCountriesHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<PagedResult<BlockedCountryDto>> Handle(
            GetBlockedCountriesQuery request,
            CancellationToken cancellationToken)
        {
            var countries = await _repository.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                request.SearchTerm,
                cancellationToken);

            var totalCount = await _repository.GetTotalCountAsync(
                request.SearchTerm,
                cancellationToken);

            var dtos = countries.Select(c => new BlockedCountryDto(
                c.Id,
                c.CountryCode.Value,
                c.CountryName,
                c.BlockedAt,
                c.IsTemporary,
                c.ExpiresAt,
                c.IsExpired()
            )).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            return new PagedResult<BlockedCountryDto>(
                dtos,
                request.PageNumber,
                request.PageSize,
                totalCount,
                totalPages
            );
        }
    }
}
