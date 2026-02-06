using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskGX.API.Models;
using TaskGX.Data;

namespace TaskGX.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly TaskGXContext _context;

        public UsuariosController(TaskGXContext context)
        {
            _context = context;
        }

        private int GetUsuarioId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(idStr) || !int.TryParse(idStr, out var id))
                throw new UnauthorizedAccessException("Token inválido ou sem ID.");
            return id;
        }

        // GET: api/usuarios/me
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var usuarioId = GetUsuarioId();

            var user = await _context.Usuarios
                .Where(u => u.ID == usuarioId)
                .Select(u => new
                {
                    u.ID,
                    u.Nome,
                    u.Email,
                    u.Avatar,
                    u.Ativo,
                    u.EmailVerificado,
                    u.CriadoEm,
                    u.DataAtualizacao
                })
                .FirstOrDefaultAsync();

            if (user == null) return Unauthorized();
            return Ok(user);
        }

        public class AtualizarMeRequest
        {
            public string Nome { get; set; } = string.Empty;
            public string? Avatar { get; set; }
        }

        // PUT: api/usuarios/me
        [HttpPut("me")]
        public async Task<IActionResult> AtualizarMe([FromBody] AtualizarMeRequest req)
        {
            var usuarioId = GetUsuarioId();

            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.ID == usuarioId);
            if (user == null) return Unauthorized();

            user.Nome = req.Nome.Trim();
            user.Avatar = req.Avatar;
            user.DataAtualizacao = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        public class AlterarSenhaRequest
        {
            public string SenhaAtual { get; set; } = string.Empty;
            public string NovaSenha { get; set; } = string.Empty;
        }

        // PATCH: api/usuarios/me/password  (opcional)
        [HttpPatch("me/password")]
        public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaRequest req)
        {
            var usuarioId = GetUsuarioId();

            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.ID == usuarioId);
            if (user == null) return Unauthorized();

            // Se você já tem PasswordService, substitui essas duas linhas por ele:
            var ok = BCrypt.Net.BCrypt.Verify(req.SenhaAtual, user.SenhaHash);
            if (!ok) return BadRequest("Senha atual inválida.");

            user.SenhaHash = BCrypt.Net.BCrypt.HashPassword(req.NovaSenha);
            user.DataAtualizacao = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
