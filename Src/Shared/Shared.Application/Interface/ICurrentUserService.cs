namespace Shared.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserName { get; }
    UserInfo? UserInfo { get; }
    int? CompanyId { get; }
    Guid? AgentId { get; }
    Guid? BranchId { get; }
    Guid? CustomerId { get; }
}
