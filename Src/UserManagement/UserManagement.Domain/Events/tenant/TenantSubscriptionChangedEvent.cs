using UserManagement.Domain.Entities.BaseEntities;

namespace UserManagement.Domain.Events.tenant;

public class TenantSubscriptionChangedEvent : BaseEvent
{
    public Guid TenantId { get; }
    public string? OldPlan { get; }
    public string? NewPlan { get; }

    public TenantSubscriptionChangedEvent(Guid tenantId, string? oldPlan, string? newPlan)
    {
        TenantId = tenantId;
        OldPlan = oldPlan;
        NewPlan = newPlan;
    }
}
