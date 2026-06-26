using MediatR;

namespace UserManagement.Domain.Entities.BaseEntities;

public abstract class BaseEvent : INotification
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
