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
        private readonly TaskGXContext _contexto;

        public ListasController(TaskGXContext contexto)
        {
            _contexto = contexto;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ListaDTO>>> ObterListas()
        {
            var usuarioId = TokenService.ObterUsuarioId(User);

            var listas = await _contexto.Listas
                .Where(lista => lista.UsuarioID == usuarioId)
                .OrderByDescending(lista => lista.Favorita)
                .ThenBy(lista => lista.Ordem)
                .ThenBy(lista => lista.Nome)
                .Select(lista => new ListaDTO
                {
                    ID = lista.ID,
                    Nome = lista.Nome,
                    Cor = lista.Cor,
                    Favorita = lista.Favorita
                })
                .ToListAsync();

            return Ok(listas);
        }

        [HttpPost]
        public async Task<ActionResult<ListaDTO>> CriarLista([FromBody] CriarListaRequest requisicao)
        {
            var usuarioId = TokenService.ObterUsuarioId(User);

            var lista = new Lista
            {
                UsuarioID = usuarioId,
                Nome = requisicao.Nome.Trim(),
                Cor = string.IsNullOrWhiteSpace(requisicao.Cor) ? null : requisicao.Cor.Trim(),
                Favorita = requisicao.Favorita,
                DataCriacao = DateTime.UtcNow
            };

            _contexto.Listas.Add(lista);
            await _contexto.SaveChangesAsync();

            var dtoLista = new ListaDTO
            {
                ID = lista.ID,
                Nome = lista.Nome,
                Cor = lista.Cor,
                Favorita = lista.Favorita
            };

            return CreatedAtAction(nameof(ObterListas), new { }, dtoLista);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarLista(int id, [FromBody] AtualizarListaRequest requisicao)
        {
            var usuarioId = TokenService.ObterUsuarioId(User);

            var lista = await _contexto.Listas.FirstOrDefaultAsync(item => item.ID == id && item.UsuarioID == usuarioId);
            if (lista == null)
                return NotFound();

            lista.Nome = requisicao.Nome.Trim();
            lista.Cor = string.IsNullOrWhiteSpace(requisicao.Cor) ? null : requisicao.Cor.Trim();
            lista.Favorita = requisicao.Favorita;

            await _contexto.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletarLista(int id)
        {
            var usuarioId = TokenService.ObterUsuarioId(User);

            var lista = await _contexto.Listas.FirstOrDefaultAsync(item => item.ID == id && item.UsuarioID == usuarioId);
            if (lista == null)
                return NotFound();

            _contexto.Listas.Remove(lista);
            await _contexto.SaveChangesAsync();
            return NoContent();
        }
    }
}
