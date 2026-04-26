using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Wallet.Core.Test.Helper;

/// <summary>توکن JWT هم‌راستا با appsettings.Test و Auth (Issuer/Audience/Secret).</summary>
public static class JwtTestHelper
{
    private const string SecretKey = "TestSecretKeyForAuthApi123456789012345";
    private const string Issuer = "AuthApi";
    private const string Audience = "AuthApiUsers";

    public static string CreateToken(Guid userId, string userType = "4")
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim("userType", userType)
            ],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
