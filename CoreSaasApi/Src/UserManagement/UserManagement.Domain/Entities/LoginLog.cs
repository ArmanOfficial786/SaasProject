namespace UserManagement.Domain.Entities;

public class LoginLog
{
    public int Id { get; private set; }
    public User User { get; private set; }
    [MaxLength(45)]
    public string IpAddress { get; private set; }
    [MaxLength(50)]
    public string? MacAddress { get; private set; }
    [MaxLength(100)]
    public string ClientAgent { get; private set; }
    [MaxLength(100)]
    public string? OS { get; private set; }
    public DateTime LoginDate { get; private set; } = DateTime.UtcNow;

    public LoginLog(User user, string ipAddress, string macAddress, string clientAgent)
    {
        User = user;
        IpAddress = ipAddress;
        MacAddress = macAddress;
        ClientAgent = clientAgent;
    }

#pragma warning disable CS8618
    public LoginLog() { }
}
