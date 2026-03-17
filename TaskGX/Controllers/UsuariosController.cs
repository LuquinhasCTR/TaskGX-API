using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskGX.API.DTOs;
using TaskGX.API.Services;
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

        // GET: api/usuarios/me
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var usuarioId = TokenService.GetUserId(User);

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

        // PUT: api/usuarios/me
        [HttpPut("me")]
        public async Task<IActionResult> AtualizarMe([FromBody] AtualizarUsuarioRequest req)
        {
            var usuarioId = TokenService.GetUserId(User);

            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.ID == usuarioId);
            if (user == null) return Unauthorized();

            user.Nome = req.Nome.Trim();
            user.Avatar = string.IsNullOrWhiteSpace(req.Avatar) ? null : req.Avatar.Trim();
            user.DataAtualizacao = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PATCH: api/usuarios/me/password
        [HttpPatch("me/password")]
        public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaRequest req)
        {
            var usuarioId = TokenService.GetUserId(User);

            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.ID == usuarioId);
            if (user == null) return Unauthorized();

            if (!PasswordService.Verify(req.SenhaAtual, user.SenhaHash))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Não foi possível alterar a senha.",
                    Detail = "Senha atual inválida.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            if (!PasswordService.IsValid(req.NovaSenha))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Não foi possível alterar a senha.",
                    Detail = "A nova senha não atende aos requisitos de segurança.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            user.SenhaHash = PasswordService.Hash(req.NovaSenha);
            user.DataAtualizacao = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
