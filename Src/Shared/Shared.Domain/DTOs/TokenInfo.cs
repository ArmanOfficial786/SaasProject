namespace Shared.Domain.DTOs;

public class TokenInfo
{
    public int UserId { get; }
    public string? UserName { get; }
    public string? FullName { get; }
    public string? Email { get; }
    public int CompanyId { get; }
    public string? AgentId { get; }
    public string? ProductCode { get; }
    public string? ProductName { get; }
    public string? ProfilePhoto { get; }
    public IList<string> Roles { get; set; }

    public TokenInfo(int userId, string? userName, string? fullName, string? email, int companyId, string? productCode, string? productName, string? profilePhoto, IList<string> roles)
    {
        UserId = userId;
        UserName = userName;
        FullName = fullName;
        Email = email;
        CompanyId = companyId;
        ProductCode = productCode;
        ProductName = productName;
        ProfilePhoto = profilePhoto;
        Roles = roles ?? [];
    }

}
