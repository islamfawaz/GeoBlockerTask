using BuildingBlocks.CQRS;
using Country.Application.Commands;
using Country.Domain.Repositories;
using Country.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Country.Application.Handlers
{

    public sealed class UnblockCountryHandler : ICommandHandler<UnblockCountryCommand, UnblockCountryResponse>
    {
        private readonly ICountryRepository _repository;
        private readonly IMediator _mediator;
        private readonly ILogger<UnblockCountryHandler> _logger;

        public UnblockCountryHandler(
            ICountryRepository repository,
            IMediator mediator,
            ILogger<UnblockCountryHandler> logger)
        {
            _repository = repository;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<UnblockCountryResponse> Handle(
            UnblockCountryCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var countryCode = CountryCode.Create(request.CountryCode);
                var country = await _repository.GetByCodeAsync(countryCode, cancellationToken);

                if (country == null)
                {
                    return new UnblockCountryResponse(
                        request.CountryCode,
                        false,
                        "Country not found in blocked list"
                    );
                }

                country.Unblock();
                await _repository.RemoveAsync(countryCode, cancellationToken);

                // Publish domain events via MediatR
                foreach (var domainEvent in country.DomainEvents)
                {
                    await _mediator.Publish(domainEvent, cancellationToken);
                }
                country.ClearDomainEvents();

                _logger.LogInformation("Country {CountryCode} unblocked successfully", request.CountryCode);

                return new UnblockCountryResponse(request.CountryCode, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unblocking country {CountryCode}", request.CountryCode);
                return new UnblockCountryResponse(request.CountryCode, false, ex.Message);
            }
        }
    }

}
