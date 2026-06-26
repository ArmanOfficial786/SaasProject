namespace Shared.Application.Interface;

public interface ITokenClaimsService
{
    string GetToken(TokenInfo? info);
}
