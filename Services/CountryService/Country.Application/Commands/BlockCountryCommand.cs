using BuildingBlocks.CQRS;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Country.Application.Commands
{
    public sealed record BlockCountryCommand(string CountryCode, string CountryName)
           : ICommand<BlockCountryResponse>;


    public sealed record BlockCountryResponse(
    Guid Id,
    string CountryCode,
    string CountryName,
    DateTime BlockedAt,
    bool Success,
    string? ErrorMessage = null
);

}
