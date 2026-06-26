using UserManagement.Domain.Entities.BaseEntities;

namespace UserManagement.Domain.Events.user;

public class UserLoggedInEvent : BaseEvent
{
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }

    public UserLoggedInEvent(Guid userId, Guid tenantId)
    {
        UserId = userId;
        TenantId = tenantId;
    }
}
