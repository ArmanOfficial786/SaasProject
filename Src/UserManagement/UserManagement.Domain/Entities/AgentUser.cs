

//which user belong to which agent

//| Id | UserId | AgentId | FromDate       | ToDate |
//| -- | ------ | ------- | ----------     | ---------- |
//| 1  | 1      | 10      | 2026 - 01 - 01 | 2026 - 03 - 31 |
//| 2  | 1      | 20      | 2026 - 04 - 01 | NULL |
//| 3  | 2      | 10      | 2026 - 06 - 01 | NULL |

namespace UserManagement.Domain.Entities;

public class AgentUser
{
    public int Id { get; private set; }
    public User User { get; private set; }
    public Agent Agent { get; private set; }
    public DateTime FromDate { get; private set; } = DateTime.UtcNow;
    public DateTime? ToDate { get; private set; }
    public void Terminate()
    {
        this.ToDate = DateTime.UtcNow;
    }

    public AgentUser(User user, Agent agent)
    {
        User = user;
        Agent = agent;
    }

#pragma warning disable CS8618
    private AgentUser() { }
}
