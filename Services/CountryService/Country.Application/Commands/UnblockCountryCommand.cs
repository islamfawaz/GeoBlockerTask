using BuildingBlocks.CQRS;
using System.Windows.Input;

namespace Country.Application.Commands
{
    public sealed record UnblockCountryCommand(string CountryCode)
        :ICommand<UnblockCountryResponse>
    {
    }


    public sealed record UnblockCountryResponse(
        string CountryCode,
        bool Success,
        string? ErrorMessage = null
    );
}
