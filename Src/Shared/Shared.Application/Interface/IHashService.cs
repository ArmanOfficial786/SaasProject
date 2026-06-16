namespace Shared.Application.Interfaces;

public interface IHashService
{
    string GetSHA256Hash(string text);
    bool VerifySHA256Hash(string text, string hash);
    string GetBcryptHash(string text);
    bool VerifyBcryptHash(string text, string hash);
}
