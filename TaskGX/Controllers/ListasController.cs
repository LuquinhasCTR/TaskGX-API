using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskGX.API.DTOs;
using TaskGX.API.Models;
using TaskGX.API.Services;
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

        // GET: api/listas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ListaDTO>>> GetListas()
        {
            var usuarioId = TokenService.GetUserId(User);

            var listas = await _context.Listas
                .Where(l => l.UsuarioID == usuarioId)
                .OrderByDescending(l => l.Favorita)
                .ThenBy(l => l.Ordem)
                .ThenBy(l => l.Nome)
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

        // POST: api/listas
        [HttpPost]
        public async Task<ActionResult<ListaDTO>> CriarLista([FromBody] CriarListaRequest request)
        {
            var usuarioId = TokenService.GetUserId(User);

            var lista = new Listas
            {
                UsuarioID = usuarioId,
                Nome = request.Nome.Trim(),
                Cor = string.IsNullOrWhiteSpace(request.Cor) ? null : request.Cor.Trim(),
                Favorita = request.Favorita,
                DataCriacao = DateTime.UtcNow
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

        // PUT: api/listas/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarLista(int id, [FromBody] AtualizarListaRequest request)
        {
            var usuarioId = TokenService.GetUserId(User);

            var lista = await _context.Listas.FirstOrDefaultAsync(l => l.ID == id && l.UsuarioID == usuarioId);
            if (lista == null) return NotFound();

            lista.Nome = request.Nome.Trim();
            lista.Cor = string.IsNullOrWhiteSpace(request.Cor) ? null : request.Cor.Trim();
            lista.Favorita = request.Favorita;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/listas/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletarLista(int id)
        {
            var usuarioId = TokenService.GetUserId(User);

            var lista = await _context.Listas.FirstOrDefaultAsync(l => l.ID == id && l.UsuarioID == usuarioId);
            if (lista == null) return NotFound();

            _context.Listas.Remove(lista);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
