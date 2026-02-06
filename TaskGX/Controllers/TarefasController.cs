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
    public class TarefasController : ControllerBase
    {
        private readonly TaskGXContext _context;

        public TarefasController(TaskGXContext context)
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

        // GET: api/tarefas?listaId=1
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TarefaDTO>>> GetTarefas(int listaId)
        {
            var usuarioId = GetUsuarioId();

            // ✅ garante que a lista pertence ao usuário
            var listaDoUsuario = await _context.Listas
                .AnyAsync(l => l.ID == listaId && l.UsuarioID == usuarioId);

            if (!listaDoUsuario) return NotFound(); // não existe ou não é dele

            var tarefas = await _context.Tarefas
                .Include(t => t.Lista)
                .Include(t => t.Prioridade)
                .Where(t => t.ListaID == listaId)
                .Select(t => new TarefaDTO
                {
                    ID = t.ID,
                    Titulo = t.Titulo,
                    Descricao = t.Descricao,
                    Tags = t.Tags,
                    Concluida = t.Concluida,
                    Arquivada = t.Arquivada,
                    DataVencimento = t.DataVencimento,
                    DataCriacao = t.DataCriacao,
                    ListaId = t.ListaID,
                    ListaNome = t.Lista != null ? t.Lista.Nome : null,
                    PrioridadeId = t.PrioridadeID,
                    PrioridadeNome = t.Prioridade != null ? t.Prioridade.Nome : null
                })
                .ToListAsync();

            return Ok(tarefas);
        }

        // POST: api/tarefas
        [HttpPost]
        public async Task<ActionResult<TarefaDTO>> CriarTarefa(Tarefas tarefa)
        {
            var usuarioId = GetUsuarioId();

            // ✅ garante que a lista é do usuário antes de criar
            var lista = await _context.Listas
                .FirstOrDefaultAsync(l => l.ID == tarefa.ListaID && l.UsuarioID == usuarioId);

            if (lista == null) return NotFound("Lista não encontrada.");

            tarefa.DataCriacao = DateTime.Now;

            _context.Tarefas.Add(tarefa);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTarefas), new { listaId = tarefa.ListaID }, new TarefaDTO
            {
                ID = tarefa.ID,
                Titulo = tarefa.Titulo,
                Descricao = tarefa.Descricao,
                Tags = tarefa.Tags,
                Concluida = tarefa.Concluida,
                Arquivada = tarefa.Arquivada,
                DataVencimento = tarefa.DataVencimento,
                DataCriacao = tarefa.DataCriacao,
                ListaId = tarefa.ListaID,
                ListaNome = lista.Nome,
                PrioridadeId = tarefa.PrioridadeID,
                PrioridadeNome = null
            });
        }

        // PUT: api/tarefas/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarTarefa(int id, Tarefas tarefa)
        {
            var usuarioId = GetUsuarioId();
            if (id != tarefa.ID) return BadRequest();

            // ✅ pega a tarefa SOMENTE se ela estiver numa lista do usuário
            var tarefaDb = await _context.Tarefas
                .Include(t => t.Lista)
                .FirstOrDefaultAsync(t => t.ID == id && t.Lista != null && t.Lista.UsuarioID == usuarioId);

            if (tarefaDb == null) return NotFound();

            // ✅ atualiza campos permitidos (evita overposting)
            tarefaDb.Titulo = tarefa.Titulo;
            tarefaDb.Descricao = tarefa.Descricao;
            tarefaDb.Tags = tarefa.Tags;
            tarefaDb.Concluida = tarefa.Concluida;
            tarefaDb.Arquivada = tarefa.Arquivada;
            tarefaDb.DataVencimento = tarefa.DataVencimento;
            tarefaDb.PrioridadeID = tarefa.PrioridadeID;

            // Se você permitir mover de lista, faça validação também:
            if (tarefaDb.ListaID != tarefa.ListaID)
            {
                var novaLista = await _context.Listas
                    .AnyAsync(l => l.ID == tarefa.ListaID && l.UsuarioID == usuarioId);

                if (!novaLista) return BadRequest("Lista destino inválida.");

                tarefaDb.ListaID = tarefa.ListaID;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/tarefas/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletarTarefa(int id)
        {
            var usuarioId = GetUsuarioId();

            var tarefa = await _context.Tarefas
                .Include(t => t.Lista)
                .FirstOrDefaultAsync(t => t.ID == id && t.Lista != null && t.Lista.UsuarioID == usuarioId);

            if (tarefa == null) return NotFound();

            _context.Tarefas.Remove(tarefa);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/tarefas/{id}/concluir
        [HttpPost("{id}/concluir")]
        public async Task<IActionResult> ConcluirTarefa(int id)
        {
            var usuarioId = GetUsuarioId();

            var tarefa = await _context.Tarefas
                .Include(t => t.Lista)
                .FirstOrDefaultAsync(t => t.ID == id && t.Lista != null && t.Lista.UsuarioID == usuarioId);

            if (tarefa == null) return NotFound();

            tarefa.Concluida = true;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
