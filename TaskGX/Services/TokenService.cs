using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TaskGX.API.Models;

namespace TaskGX.API.Services
{
    public class TokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public string CreateToken(Usuarios usuario)
        {
            if (usuario == null) throw new ArgumentNullException(nameof(usuario));

            var key = _config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key não configurado.");
            var issuer = _config["Jwt:Issuer"] ?? "TaskGX";
            var audience = _config["Jwt:Audience"] ?? "TaskGX";

            // Padronize com o mesmo nome que você usar no appsettings.json
            // Recomendado: "ExpireMinutes"
            var expiresMinutesStr = _config["Jwt:ExpireMinutes"];
            var expiresMinutes = int.TryParse(expiresMinutesStr, out var m) ? m : 480;

            var claims = new List<Claim>
            {
                // ID do usuário (compatível com o ecossistema ASP.NET)
                new(JwtRegisteredClaimNames.Sub, usuario.ID.ToString()),
                new(ClaimTypes.NameIdentifier, usuario.ID.ToString()),

                // Email
                new(JwtRegisteredClaimNames.Email, usuario.Email),
                new(ClaimTypes.Email, usuario.Email),

                // Nome (opcional)
                new("name", usuario.Nome),
                new(ClaimTypes.Name, usuario.Nome),
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Pega o ID do usuário do ClaimsPrincipal.
        /// Prioriza ClaimTypes.NameIdentifier, e cai no "sub" como fallback.
        /// </summary>
        public static int GetUserId(ClaimsPrincipal user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

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
