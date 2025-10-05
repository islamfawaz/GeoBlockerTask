using BuildingBlocks.CQRS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Country.Application.Commands
{
    public record BlockCountryTemporarilyCommand(string CountryCode, string CountryName, int DurationMinutes)
        :ICommand<BlockCountryResponse>
    {
    }
}
