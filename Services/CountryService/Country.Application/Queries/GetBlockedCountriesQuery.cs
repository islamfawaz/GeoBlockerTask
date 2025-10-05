using BuildingBlocks.CQRS;
using Country.Application._DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Country.Application.Queries
{
    public record GetBlockedCountriesQuery(int PageNumber = 1, int PageSize = 20,string? SearchTerm = null)
 : IQuery<PagedResult<BlockedCountryDto>>;

  public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages
);
}
