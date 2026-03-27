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
    public class TarefasController : ControllerBase
    {
        private readonly TaskGXContext _context;

        public TarefasController(TaskGXContext context)
        {
            _context = context;
        }

        // GET: api/tarefas?listaId=1
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TarefaDTO>>> GetTarefas([FromQuery] int listaId)
        {
            var usuarioId = TokenService.GetUserId(User);

            var listaDoUsuario = await _context.Listas.AnyAsync(l => l.ID == listaId && l.UsuarioID == usuarioId);
            if (!listaDoUsuario) return NotFound();

            var tarefas = await _context.Tarefas
                .Include(t => t.Lista)
                .Include(t => t.Prioridade)
                .Where(t => t.ListaID == listaId)
                .OrderBy(t => t.Concluida)
                .ThenBy(t => t.Ordem)
                .ThenBy(t => t.DataCriacao)
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
        public async Task<ActionResult<TarefaDTO>> CriarTarefa([FromBody] CriarTarefaRequest request)
        {
            var usuarioId = TokenService.GetUserId(User);

            var lista = await _context.Listas
                .FirstOrDefaultAsync(l => l.ID == request.ListaID && l.UsuarioID == usuarioId);

            if (lista == null) return NotFound("Lista nao encontrada.");

            if (request.PrioridadeID.HasValue)
            {
                var prioridadeExiste = await _context.Prioridades.AnyAsync(p => p.ID == request.PrioridadeID.Value);
                if (!prioridadeExiste) return BadRequest("Prioridade invalida.");
            }

            var tarefa = new Tarefas
            {
                ListaID = request.ListaID,
                Titulo = request.Titulo.Trim(),
                Descricao = string.IsNullOrWhiteSpace(request.Descricao) ? null : request.Descricao.Trim(),
                Tags = string.IsNullOrWhiteSpace(request.Tags) ? null : request.Tags.Trim(),
                PrioridadeID = request.PrioridadeID,
                Concluida = request.Concluida,
                Arquivada = request.Arquivada,
                DataVencimento = request.DataVencimento,
                Ordem = request.Ordem,
                DataCriacao = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow
            };

            _context.Tarefas.Add(tarefa);
            await _context.SaveChangesAsync();

            string? prioridadeNome = null;
            if (tarefa.PrioridadeID.HasValue)
            {
                prioridadeNome = await _context.Prioridades
                    .Where(p => p.ID == tarefa.PrioridadeID.Value)
                    .Select(p => p.Nome)
                    .FirstOrDefaultAsync();
            }

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
                PrioridadeNome = prioridadeNome
            });
        }

        // PUT: api/tarefas/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarTarefa(int id, [FromBody] AtualizarTarefaRequest request)
        {
            var usuarioId = TokenService.GetUserId(User);
            if (id != request.ID) return BadRequest();

            var tarefaDb = await _context.Tarefas
                .Include(t => t.Lista)
                .FirstOrDefaultAsync(t => t.ID == id && t.Lista != null && t.Lista.UsuarioID == usuarioId);

            if (tarefaDb == null) return NotFound();

            if (request.PrioridadeID.HasValue)
            {
                var prioridadeExiste = await _context.Prioridades.AnyAsync(p => p.ID == request.PrioridadeID.Value);
                if (!prioridadeExiste) return BadRequest("Prioridade invalida.");
            }

            tarefaDb.Titulo = request.Titulo.Trim();
            tarefaDb.Descricao = string.IsNullOrWhiteSpace(request.Descricao) ? null : request.Descricao.Trim();
            tarefaDb.Tags = string.IsNullOrWhiteSpace(request.Tags) ? null : request.Tags.Trim();
            tarefaDb.Concluida = request.Concluida;
            tarefaDb.Arquivada = request.Arquivada;
            tarefaDb.DataVencimento = request.DataVencimento;
            tarefaDb.PrioridadeID = request.PrioridadeID;
            tarefaDb.Ordem = request.Ordem;
            tarefaDb.DataAtualizacao = DateTime.UtcNow;

            if (tarefaDb.ListaID != request.ListaID)
            {
                var novaLista = await _context.Listas.AnyAsync(l => l.ID == request.ListaID && l.UsuarioID == usuarioId);
                if (!novaLista) return BadRequest("Lista destino invalida.");

                tarefaDb.ListaID = request.ListaID;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/tarefas/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletarTarefa(int id)
        {
            var usuarioId = TokenService.GetUserId(User);

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
            var usuarioId = TokenService.GetUserId(User);

            var tarefa = await _context.Tarefas
                .Include(t => t.Lista)
                .FirstOrDefaultAsync(t => t.ID == id && t.Lista != null && t.Lista.UsuarioID == usuarioId);

            if (tarefa == null) return NotFound();

            tarefa.Concluida = true;
            tarefa.DataAtualizacao = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
