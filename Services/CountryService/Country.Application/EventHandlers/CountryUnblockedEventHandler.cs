using Country.Domain.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Country.Application.EventHandlers
{

    public sealed class CountryUnblockedEventHandler : INotificationHandler<CountryUnblockedEvent>
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<CountryUnblockedEventHandler> _logger;

        public CountryUnblockedEventHandler(
            IPublishEndpoint publishEndpoint,
            ILogger<CountryUnblockedEventHandler> logger)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task Handle(CountryUnblockedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Handling CountryUnblockedEvent for {CountryCode}",
                notification.CountryCode);

            // Publish integration event to message bus via MassTransit
            await _publishEndpoint.Publish(notification.ToIntegrationEvent(), cancellationToken);

            _logger.LogInformation(
                "Published CountryUnblockedEvent to message bus for {CountryCode}",
                notification.CountryCode);
        }
    }
}
