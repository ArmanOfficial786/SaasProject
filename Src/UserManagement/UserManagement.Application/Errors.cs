using Shared.Domain.DTOs;

namespace UserManagement.Application;

public static class Errors
{
    public static ErrorDTO Unauthorized => new("UNAUTHORIZED", "Authentication required.");
    public static ErrorDTO RoleIsRequired => new("ROLE_REQUIRED", "At least one role must be assigned.");
    public static ErrorDTO CompanyNotFound => new("COMPANY_NOT_FOUND", "Company not found.");
    public static ErrorDTO AgentNotFound => new("AGENT_NOT_FOUND", "Agent not found for this company.");
    public static ErrorDTO Exception(Exception ex) => new("EXCEPTION", ex.Message);
}
