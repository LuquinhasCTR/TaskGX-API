using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaskGX.API.Models;

namespace TaskGX.API.Services
{
    public class TokenService
    {
        private readonly JwtSettings _settings;

        public TokenService(IOptions<JwtSettings> settings)
        {
            _settings = settings.Value;
        }

        public string CreateToken(Usuarios usuario)
        {
            ArgumentNullException.ThrowIfNull(usuario);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, usuario.ID.ToString()),
                new(ClaimTypes.NameIdentifier, usuario.ID.ToString()),
                new(JwtRegisteredClaimNames.Email, usuario.Email),
                new(ClaimTypes.Email, usuario.Email),
                new("name", usuario.Nome),
                new(ClaimTypes.Name, usuario.Nome),
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(_settings.ExpireMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static int GetUserId(ClaimsPrincipal user)
        {
            ArgumentNullException.ThrowIfNull(user);

            var idStr =
                user.FindFirstValue(ClaimTypes.NameIdentifier) ??
                user.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrWhiteSpace(idStr) || !int.TryParse(idStr, out var id))
                throw new InvalidOperationException("Token sem ID de usuário (NameIdentifier/sub).");

            return id;
        }

        public static string? GetEmail(ClaimsPrincipal user)
        {
            if (user == null) return null;

            return user.FindFirstValue(ClaimTypes.Email)
                ?? user.FindFirstValue(JwtRegisteredClaimNames.Email);
        }

        public static string? GetName(ClaimsPrincipal user)
        {
            if (user == null) return null;

            return user.FindFirstValue(ClaimTypes.Name)
                ?? user.FindFirst("name")?.Value;
        }
    }
}
