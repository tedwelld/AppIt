using AppIt.Core.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AppIt.Api.Services
{
    public class JwtTokenService
    {
        private readonly IConfiguration _config;

        public JwtTokenService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(AccountDto user)
        {
            var key = _config["Auth:JwtKey"]
                ?? throw new InvalidOperationException("Auth:JwtKey is not configured.");
            var issuer = _config["Auth:JwtIssuer"] ?? "AppIt";
            var audience = _config["Auth:JwtAudience"] ?? "AppItClient";
            var expireMinutes = int.TryParse(_config["Auth:JwtExpireMinutes"], out var m) ? m : 1440;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var roleName = string.IsNullOrWhiteSpace(user.Role) ? "regular" : user.Role;

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, BuildDisplayName(user)),
                new Claim(ClaimTypes.GivenName, user.FirstName ?? string.Empty),
                new Claim(ClaimTypes.Surname, user.LastName ?? string.Empty),
                // Specific role name drives client-side menu/feature visibility.
                new Claim(ClaimTypes.Role, roleName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Back-office staff roles also receive the "admin" tier claim so existing
            // role-gated endpoints (Roles = "super,admin") keep authorizing them.
            if (AppIt.Core.Authorization.RoleCatalog.IsBackOffice(roleName)
                && !roleName.Equals(AppIt.Core.Authorization.RoleCatalog.AdminRole, StringComparison.OrdinalIgnoreCase))
            {
                claims.Add(new Claim(ClaimTypes.Role, AppIt.Core.Authorization.RoleCatalog.AdminRole));
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string BuildDisplayName(AccountDto user)
        {
            var displayName = $"{user.FirstName} {user.LastName}".Trim();
            return string.IsNullOrWhiteSpace(displayName) ? user.Email : displayName;
        }
    }
}
