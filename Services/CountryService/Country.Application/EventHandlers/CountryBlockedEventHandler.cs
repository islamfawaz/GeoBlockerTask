using Country.Domain.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Country.Application.EventHandlers
{
    public sealed class CountryBlockedEventHandler : INotificationHandler<CountryBlockedEvent>
    {
        private readonly IPublishEndpoint _eventPublisher;
        private readonly ILogger<CountryBlockedEventHandler> _logger;

        public CountryBlockedEventHandler(
            IPublishEndpoint eventPublisher,
            ILogger<CountryBlockedEventHandler> logger)
        {
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        public async Task Handle(CountryBlockedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Handling CountryBlockedEvent for {CountryCode}",
                notification.CountryCode);

            // Publish integration event to RabbitMQ for external services
            await _eventPublisher.Publish(notification.ToIntegrationEvent(), cancellationToken);

            _logger.LogInformation(
                "Published CountryBlockedEvent to message bus for {CountryCode}",
                notification.CountryCode);
        }
    }
}
