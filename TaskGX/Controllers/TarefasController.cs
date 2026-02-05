using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskGX.Data;
using TaskGX.API.DTOs;
using TaskGX.API.Models;

namespace TaskGX.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TarefasController : ControllerBase
    {
        private readonly TaskGXContext _context;

        public TarefasController(TaskGXContext context)
        {
            _context = context;
        }

        // GET: api/tarefas?listaId=1
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TarefaDTO>>> GetTarefas(int listaId)
        {
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

            return tarefas;
        }

        // POST: api/tarefas
        [HttpPost]
        public async Task<ActionResult<TarefaDTO>> CriarTarefa(Tarefas tarefa)
        {
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
                ListaNome = null,
                PrioridadeId = tarefa.PrioridadeID,
                PrioridadeNome = null
            });
        }

        // PUT: api/tarefas/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarTarefa(int id, Tarefas tarefa)
        {
            if (id != tarefa.ID) return BadRequest();

            _context.Entry(tarefa).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/tarefas/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletarTarefa(int id)
        {
            var tarefa = await _context.Tarefas.FindAsync(id);
            if (tarefa == null) return NotFound();

            _context.Tarefas.Remove(tarefa);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/tarefas/{id}/concluir
        [HttpPost("{id}/concluir")]
        public async Task<IActionResult> ConcluirTarefa(int id)
        {
            var tarefa = await _context.Tarefas.FindAsync(id);
            if (tarefa == null) return NotFound();

            tarefa.Concluida = true;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
