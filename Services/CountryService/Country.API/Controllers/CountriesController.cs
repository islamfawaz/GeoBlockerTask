using Country.Application.Commands;
using Country.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Country.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CountriesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public CountriesController(IMediator mediator) => _mediator = mediator;

        [HttpPost("block")]    
        public async Task<IActionResult> Block([FromBody] BlockCountryCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("temporal-block")]
        public async Task<IActionResult> BlockTemporary([FromBody] BlockCountryTemporarilyCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("block/{countryCode}")]
        public async Task<IActionResult> Unblock([FromRoute] string countryCode)
        {
            var result = await _mediator.Send(new UnblockCountryCommand(countryCode));
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("blocked")]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        {
            var result = await _mediator.Send(new GetBlockedCountriesQuery(page, pageSize, search));
            return Ok(result);
        }

        [HttpGet("{countryCode}")]
        public async Task<IActionResult> GetByCode([FromRoute] string countryCode)
        {
            var result = await _mediator.Send(new GetCountryByCodeQuery(countryCode));
            return Ok(result);
        }
    }
}


