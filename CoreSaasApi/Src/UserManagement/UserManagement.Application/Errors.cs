namespace UserManagement.Application;

public static class Errors
{
    public static ErrorDTO Unauthorized => new("UNAUTHORIZED", "Authentication required.");
    public static ErrorDTO RoleIsRequired => new("ROLE_REQUIRED", "At least one role must be assigned.");
    public static ErrorDTO CompanyNotFound => new("COMPANY_NOT_FOUND", "Company not found.");
    public static ErrorDTO AgentNotFound => new("AGENT_NOT_FOUND", "Agent not found for this company.");
    public static ErrorDTO Exception(Exception ex) => new("EXCEPTION", ex.Message);

    #region Company  Errorse
    public static ErrorDTO CompanyAlreadyExists => new("COMPANY_ALREADY_EXISTS", "A company with the same PAN, Registration Number, or Email already exists.");
    #endregion
    #region Branch Errors
    public static ErrorDTO BranchAlreadyExists => new("Branch_Already_Exists", "Branch already exists.");
    #endregion
    #region User Errors
    public static ErrorDTO UserAlreadyExists => new("User_Already_Exists", "User already exists.");
    #endregion
    #region Role Errors
    public static ErrorDTO RoleAlreadyExists => new("Role_Already_Exists", "Role already exists for this user.");
    #endregion
}
