using System.ComponentModel.DataAnnotations.Schema;
using Shared.Domain.Abstractions;

namespace UserManagement.Domain.Entities.BaseEntities;

public abstract class BaseEntity : IHasDomainEvents
{
    public Guid Id { get; set; } = Guid.NewGuid();

    private readonly List<BaseEvent> _domainEvents = [];

    [NotMapped]
    public IReadOnlyCollection<BaseEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(BaseEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(BaseEvent domainEvent)
    {
        _ = _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
