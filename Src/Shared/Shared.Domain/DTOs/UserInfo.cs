namespace Shared.Domain.DTOs;

public class UserInfo(Guid id, string userName, string name)
{
    public Guid Id { get; set; } = id;
    public string UserName { get; set; } = userName;
    public string Name { get; set; } = name;
}
