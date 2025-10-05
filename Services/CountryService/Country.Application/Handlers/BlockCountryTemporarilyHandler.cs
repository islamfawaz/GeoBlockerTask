using BuildingBlocks.CQRS;
using Country.Application.Commands;
using Country.Domain.Entities;
using Country.Domain.Repositories;
using Country.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Country.Application.Handlers
{
    public sealed class BlockCountryTemporarilyHandler
     : ICommandHandler<BlockCountryTemporarilyCommand, BlockCountryResponse>
    {
        private readonly ICountryRepository _repository;
        private readonly IMediator _mediator;
        private readonly ILogger<BlockCountryTemporarilyHandler> _logger;

        public BlockCountryTemporarilyHandler(
            ICountryRepository repository,
            IMediator mediator,
            ILogger<BlockCountryTemporarilyHandler> logger)
        {
            _repository = repository;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<BlockCountryResponse> Handle(
            BlockCountryTemporarilyCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var countryCode = CountryCode.Create(request.CountryCode);

                if (await _repository.ExistsAsync(countryCode, cancellationToken))
                {
                    return new BlockCountryResponse(
                        Guid.Empty,
                        request.CountryCode,
                        request.CountryName,
                        DateTime.UtcNow,
                        false,
                        "Country is already blocked"
                    );
                }

                var blockedCountry = BlockedCountry.BlockTemporarily(
                    countryCode,
                    request.CountryName,
                    request.DurationMinutes);

                await _repository.AddAsync(blockedCountry, cancellationToken);

                // Publish domain events via MediatR
                foreach (var domainEvent in blockedCountry.DomainEvents)
                {
                    await _mediator.Publish(domainEvent, cancellationToken);
                }
                blockedCountry.ClearDomainEvents();

                _logger.LogInformation(
                    "Country {CountryCode} temporarily blocked for {Duration} minutes",
                    request.CountryCode,
                    request.DurationMinutes);

                return new BlockCountryResponse(
                    blockedCountry.Id,
                    blockedCountry.CountryCode.Value,
                    blockedCountry.CountryName,
                    blockedCountry.BlockedAt,
                    true
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error temporarily blocking country {CountryCode}", request.CountryCode);
                return new BlockCountryResponse(
                    Guid.Empty,
                    request.CountryCode,
                    request.CountryName,
                    DateTime.UtcNow,
                    false,
                    ex.Message
                );
            }
        }
    }

}
