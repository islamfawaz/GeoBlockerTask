using MediatR;

namespace Country.Domain.Common
{
    public interface IDomainEvent :INotification
    {
        Guid EventId { get; }
        DateTime OccurredOn { get; }
    }
}
