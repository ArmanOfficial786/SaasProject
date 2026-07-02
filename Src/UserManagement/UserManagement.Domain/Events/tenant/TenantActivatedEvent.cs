


namespace UserManagement.Domain.Events.tenant;

public class TenantActivatedEvent : BaseEvent
{
    public Guid TenantId { get; }

    public TenantActivatedEvent(Guid tenantId)
    {
        TenantId = tenantId;
    }
}
