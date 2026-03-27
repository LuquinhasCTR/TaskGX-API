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
        private const string ValidationProblemType = "https://tools.ietf.org/html/rfc9110#section-15.5.1";

        private readonly TaskGXContext _context;
        private readonly EmailChangeService _emailChangeService;

        public UsuariosController(TaskGXContext context, EmailChangeService emailChangeService)
        {
            _context = context;
            _emailChangeService = emailChangeService;
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

            var nome = (req.Nome ?? string.Empty).Trim();
            var avatar = string.IsNullOrWhiteSpace(req.Avatar) ? null : req.Avatar.Trim();

            var erros = new Dictionary<string, string[]>();

            if (string.IsNullOrWhiteSpace(nome))
            {
                erros["nome"] = ["O nome e obrigatorio."];
            }
            else
            {
                if (nome.Length < 2)
                    erros["nome"] = ["O nome deve ter pelo menos 2 caracteres."];
                else if (nome.Length > 100)
                    erros["nome"] = ["O nome deve ter no maximo 100 caracteres."];
            }

            if (!string.IsNullOrEmpty(avatar) && avatar.Length > 255)
                erros["avatar"] = ["O avatar deve ter no maximo 255 caracteres."];

            if (erros.Count > 0)
                return BadRequest(CreateValidationProblem(erros));

            user.Nome = nome;
            user.Avatar = avatar;
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
                return BadRequest(CreateProblem(
                    "Nao foi possivel alterar a senha.",
                    "A senha atual informada esta incorreta.",
                    StatusCodes.Status400BadRequest));
            }

            if (PasswordService.Verify(req.NovaSenha, user.SenhaHash))
            {
                return BadRequest(CreateProblem(
                    "Nao foi possivel alterar a senha.",
                    "A nova senha deve ser diferente da senha atual.",
                    StatusCodes.Status400BadRequest));
            }

            if (!string.Equals(req.NovaSenha, req.ConfirmarNovaSenha, StringComparison.Ordinal))
            {
                return BadRequest(CreateProblem(
                    "Nao foi possivel alterar a senha.",
                    "A confirmacao da nova senha nao confere.",
                    StatusCodes.Status400BadRequest));
            }

            if (!PasswordService.IsValid(req.NovaSenha))
            {
                return BadRequest(CreateProblem(
                    "Nao foi possivel alterar a senha.",
                    "A nova senha nao atende aos requisitos de seguranca.",
                    StatusCodes.Status400BadRequest));
            }

            user.SenhaHash = PasswordService.Hash(req.NovaSenha);
            user.DataAtualizacao = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/usuarios/me/email/request-change
        [HttpPost("me/email/request-change")]
        public async Task<IActionResult> SolicitarAlteracaoEmail([FromBody] SolicitarAlteracaoEmailRequest req)
        {
            var usuarioId = TokenService.GetUserId(User);
            var resultado = await _emailChangeService.SolicitarAlteracaoAsync(usuarioId, req.NovoEmail);

            if (!resultado.Sucesso)
            {
                var problem = CreateProblem(
                    "Nao foi possivel solicitar a alteracao de email.",
                    resultado.Mensagem,
                    resultado.StatusCode);

                if (resultado.StatusCode == StatusCodes.Status401Unauthorized)
                    return Unauthorized(problem);

                return StatusCode(resultado.StatusCode, problem);
            }

            return Ok(new { message = resultado.Mensagem });
        }

        // POST: api/usuarios/me/email/confirm-change
        [HttpPost("me/email/confirm-change")]
        public async Task<IActionResult> ConfirmarAlteracaoEmail([FromBody] ConfirmarAlteracaoEmailRequest req)
        {
            var usuarioId = TokenService.GetUserId(User);
            var resultado = await _emailChangeService.ConfirmarAlteracaoAsync(usuarioId, req.Codigo);

            if (!resultado.Sucesso)
            {
                var problem = CreateProblem(
                    "Nao foi possivel confirmar a alteracao de email.",
                    resultado.Mensagem,
                    resultado.StatusCode);

                if (resultado.StatusCode == StatusCodes.Status401Unauthorized)
                    return Unauthorized(problem);

                return StatusCode(resultado.StatusCode, problem);
            }

            return Ok(new { message = resultado.Mensagem });
        }

        private static ProblemDetails CreateProblem(string title, string detail, int statusCode)
        {
            return new ProblemDetails
            {
                Title = title,
                Detail = detail,
                Status = statusCode
            };
        }

        private static ValidationProblemDetails CreateValidationProblem(IDictionary<string, string[]> erros)
        {
            return new ValidationProblemDetails(erros)
            {
                Title = "A requisicao contem dados invalidos.",
                Status = StatusCodes.Status400BadRequest,
                Type = ValidationProblemType
            };
        }
    }
}
