using System.Security.Cryptography;
using System.Text;
using BCrypter = BCrypt.Net.BCrypt;

namespace Shared.Infrastructure.Services;

public class HashService : IHashService
{
    public string GetSHA256Hash(string text)
    {
        byte[] textToByte = Encoding.UTF8.GetBytes(text);
        byte[] hashedByte = SHA256.HashData(textToByte);
        string hash = Convert.ToHexString(hashedByte);
        return hash;
    }

    public bool VerifySHA256Hash(string text, string hash)
    {
        string hashedText = this.GetSHA256Hash(text);
        return hashedText.Equals(hash);
    }

    public string GetBcryptHash(string text)
    {
        return BCrypter.HashPassword(text);
    }

    public bool VerifyBcryptHash(string text, string hash)
    {
        return BCrypter.Verify(text, hash);
    }
}
