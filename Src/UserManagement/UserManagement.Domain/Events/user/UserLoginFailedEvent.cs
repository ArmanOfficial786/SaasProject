namespace UserManagement.Domain.Events.user;

public class UserLoginFailedEvent : BaseEvent
{
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }

    public UserLoginFailedEvent(Guid userId, Guid tenantId)
    {
        UserId = userId;
        TenantId = tenantId;
    }
}
