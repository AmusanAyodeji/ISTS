using System.Security.Cryptography;
using System.Text;

namespace Ticketing.Application.Common.Security;

public static class TokenHashing
{
    public static string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}
