namespace Shared.Domain.DTOs;

public class UserInfo(Guid id, string userName, string name, int companyId)
{
    public Guid Id { get; set; } = id;
    public string UserName { get; set; } = userName;
    public string Name { get; set; } = name;
    public int CompanyId { get; set; } = companyId;
}
