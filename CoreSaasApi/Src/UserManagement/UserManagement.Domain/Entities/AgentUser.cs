//which user belong to which agent

namespace UserManagement.Domain.Entities;

public class AgentUser
{
    public int Id { get; private set; }
    public Guid UserId { get; private set; }
    public User? User { get; private set; }
    public Guid AgentId { get; private set; }
    public Agent? Agent { get; private set; }
    public DateTime FromDate { get; private set; } = DateTime.UtcNow;
    public DateTime? ToDate { get; private set; }

    public void Terminate() => ToDate = DateTime.UtcNow;

    private AgentUser() { }

    public AgentUser(Guid userId, Guid agentId)
    {
        UserId = userId;
        AgentId = agentId;
    }
}
