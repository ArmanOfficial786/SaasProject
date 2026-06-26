using UserManagement.Domain.Entities.BaseEntities;

namespace UserManagement.Domain.Events.tenant;

public class TenantCreatedEvent : BaseEvent
{
    public Guid TenantId { get; }
    public string ProductCode { get; }
    public TenantCreatedEvent(Guid tenantId, string productCode)
    {
        TenantId = tenantId;
        ProductCode = productCode;
    }


}
