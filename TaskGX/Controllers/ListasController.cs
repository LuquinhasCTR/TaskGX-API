using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using TaskGX.API.Data;
using TaskGX.API.DTOs;
using TaskGX.API.Models;

namespace TaskGX.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ListasController : ControllerBase
    {
        private readonly TaskGXContext _context;

        public ListasController(TaskGXContext context)
        {
            _context = context;
        }

        // GET: api/listas?usuarioId=1
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ListaDTO>>> GetListas(int usuarioId)
        {
            var listas = await _context.Listas
                .Where(l => l.UsuarioId == usuarioId)
                .Select(l => new ListaDTO
                {
                    ID = l.ID,
                    Nome = l.Nome,
                    Cor = l.Cor,
                    Favorita = l.Favorita
                })
                .ToListAsync();

            return listas;
        }

        // POST: api/listas
        [HttpPost]
        public async Task<ActionResult<ListaDTO>> CriarLista(Listas lista)
        {
            lista.DataCriacao = DateTime.Now;
            _context.Listas.Add(lista);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetListas), new { usuarioId = lista.UsuarioId }, new ListaDTO
            {
                ID = lista.ID,
                Nome = lista.Nome,
                Cor = lista.Cor,
                Favorita = lista.Favorita
            });
        }

        // PUT: api/listas/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarLista(int id, Listas lista)
        {
            if (id != lista.ID) return BadRequest();

            _context.Entry(lista).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/listas/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletarLista(int id)
        {
            var lista = await _context.Listas.FindAsync(id);
            if (lista == null) return NotFound();

            _context.Listas.Remove(lista);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
