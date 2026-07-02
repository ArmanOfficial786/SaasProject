namespace Shared.Domain.Abstractions;

public sealed class DomainEventCollection
{
    private readonly List<BaseEvent> _events = [];
    public IReadOnlyCollection<BaseEvent> Events => _events.AsReadOnly();

    public void Add(BaseEvent domainEvent) => _events.Add(domainEvent);
    public void Clear() => _events.Clear();
}
