using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Application.Configuration;

namespace Shared.Infrastructure.Service;

public class IdentityTokenClaimService : ITokenClaimsService
{

    private readonly UserManager<User> _userManager;
    private readonly AppConfig _config;

    public IdentityTokenClaimService(UserManager<User> userManager, IOptions<AppConfig> config)
    {
        _userManager = userManager;
        _config = config.Value;
    }


    public string GetToken(TokenInfo? info)
    {
        ArgumentNullException.ThrowIfNull(info);
        ArgumentNullException.ThrowIfNull(info.Email);
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_config.ApiKey);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, info.Email ?? string.Empty),
            new Claim("UserId", info.UserId.ToString() ?? string.Empty),
            new Claim("UserName", info.UserName ?? string.Empty),
            new Claim("Name", info.FullName ?? string.Empty),
            new Claim("CompanyId", info.CompanyId.ToString() ?? string.Empty),
            new Claim("AgentId", info.AgentId ?? string.Empty),
            new Claim("ProductCode", info.ProductCode ?? string.Empty),
        };

        var roles = info.Roles;
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _config.ApiURL,
            Audience = _config.WebURL,
            Subject = new ClaimsIdentity(claims.ToArray()),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
