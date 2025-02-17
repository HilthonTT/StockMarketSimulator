using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using StockMarketSimulator.Api.Modules.Roles.Api;
using StockMarketSimulator.Api.Modules.Users.Domain;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace StockMarketSimulator.Api.Modules.Users.Infrastructure;

internal sealed class TokenProvider : ITokenProvider
{
    private readonly JwtOptions _jwtOptions;
    private readonly IRolesApi _rolesApi;

    public TokenProvider(
        IOptions<JwtOptions> jwtOptions,
        IRolesApi rolesApi)
    {
        _jwtOptions = jwtOptions.Value;
        _rolesApi = rolesApi;
    }

    public async Task<string> Create(NpgsqlConnection connection, User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        List<string> roleNames = await _rolesApi.GetUserRoleNamesByUserIdAsync(connection, user.Id);

        List<Claim> claims = 
        [
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.PreferredUsername, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
        ];

        claims.AddRange(roleNames.Select(r => new Claim(ClaimTypes.Role, r)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationInMinutes),
            SigningCredentials = credentials,
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
        };

        var handler = new JsonWebTokenHandler();

        string token = handler.CreateToken(tokenDescriptor);

        return token;
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }
}
