using BuildingBlocks.CQRS;
using Country.Application.Commands;
using Country.Domain.Repositories;
using Country.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Country.Application.Handlers
{
    public sealed class BlockCountryHandler : ICommandHandler<BlockCountryCommand, BlockCountryResponse>
    {
        private readonly ICountryRepository _repository;
        private readonly IMediator _mediator;
        private readonly ILogger<BlockCountryHandler> _logger;

        public BlockCountryHandler(
            ICountryRepository repository,
            IMediator mediator,
            ILogger<BlockCountryHandler> logger)
        {
            _repository = repository;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<BlockCountryResponse> Handle(
            BlockCountryCommand request,
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

                var blockedCountry = Domain.Entities.BlockedCountry.Block(countryCode, request.CountryName);
                await _repository.AddAsync(blockedCountry, cancellationToken);

                // Publish domain events via MediatR
                foreach (var domainEvent in blockedCountry.DomainEvents)
                {
                    await _mediator.Publish(domainEvent, cancellationToken);
                }
                blockedCountry.ClearDomainEvents();

                _logger.LogInformation(
                    "Country {CountryCode} ({CountryName}) blocked successfully",
                    request.CountryCode,
                    request.CountryName);

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
                _logger.LogError(ex, "Error blocking country {CountryCode}", request.CountryCode);
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
