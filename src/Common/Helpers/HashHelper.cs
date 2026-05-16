using System.Security.Cryptography;
using System.Text;

namespace OrderApi.Common.Helpers;

public static class HashHelper
{
    public static string ToSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLower();
    }
}