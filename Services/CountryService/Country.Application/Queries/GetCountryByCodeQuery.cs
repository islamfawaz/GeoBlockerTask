using BuildingBlocks.CQRS;
using Country.Application._DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Country.Application.Queries
{
    public sealed record GetCountryByCodeQuery(string CountryCode)
        :IQuery<BlockedCountryDto>;

}
