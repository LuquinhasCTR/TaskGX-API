using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskGX.API.DTOs;
using TaskGX.API.Models;
using TaskGX.Data;

namespace TaskGX.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ListasController : ControllerBase
    {
        private readonly TaskGXContext _context;

        public ListasController(TaskGXContext context)
        {
            _context = context;
        }

        private int GetUsuarioId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdStr) || !int.TryParse(userIdStr, out var userId))
                throw new UnauthorizedAccessException("Token inválido ou sem ID de usuário.");
            return userId;
        }

        // GET: api/listas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ListaDTO>>> GetListas()
        {
            var usuarioId = GetUsuarioId();

            var listas = await _context.Listas
                .Where(l => l.UsuarioID == usuarioId)
                .Select(l => new ListaDTO
                {
                    ID = l.ID,
                    Nome = l.Nome,
                    Cor = l.Cor,
                    Favorita = l.Favorita
                })
                .ToListAsync();

            return Ok(listas);
        }

        public class CriarListaRequest
        {
            public string Nome { get; set; } = string.Empty;
            public string? Cor { get; set; }
            public bool Favorita { get; set; } = false;
        }

        // POST: api/listas
        [HttpPost]
        public async Task<ActionResult<ListaDTO>> CriarLista([FromBody] CriarListaRequest request)
        {
            var usuarioId = GetUsuarioId();

            var lista = new Listas
            {
                UsuarioID = usuarioId,
                Nome = request.Nome,
                Cor = request.Cor,
                Favorita = request.Favorita,
                DataCriacao = DateTime.Now
            };

            _context.Listas.Add(lista);
            await _context.SaveChangesAsync();

            var dto = new ListaDTO
            {
                ID = lista.ID,
                Nome = lista.Nome,
                Cor = lista.Cor,
                Favorita = lista.Favorita
            };

            return CreatedAtAction(nameof(GetListas), new { }, dto);
        }

        public class AtualizarListaRequest
        {
            public string Nome { get; set; } = string.Empty;
            public string? Cor { get; set; }
            public bool Favorita { get; set; }
        }

        // PUT: api/listas/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarLista(int id, [FromBody] AtualizarListaRequest request)
        {
            var usuarioId = GetUsuarioId();

            var lista = await _context.Listas.FirstOrDefaultAsync(l => l.ID == id && l.UsuarioID == usuarioId);
            if (lista == null) return NotFound(); // não existe ou não é do usuário

            lista.Nome = request.Nome;
            lista.Cor = request.Cor;
            lista.Favorita = request.Favorita;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/listas/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletarLista(int id)
        {
            var usuarioId = GetUsuarioId();

            var lista = await _context.Listas.FirstOrDefaultAsync(l => l.ID == id && l.UsuarioID == usuarioId);
            if (lista == null) return NotFound();

            _context.Listas.Remove(lista);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
