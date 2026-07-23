using MediatR;

namespace Shared.Domain.Abstractions;

public abstract class BaseEvent : INotification
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
