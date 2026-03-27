using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskGX.Data;

namespace TaskGX.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PrioridadesController : ControllerBase
    {
        private readonly TaskGXContext _contexto;

        public PrioridadesController(TaskGXContext contexto)
        {
            _contexto = contexto;
        }

        [HttpGet]
        public async Task<IActionResult> Obter()
        {
            var prioridades = await _contexto.Prioridades
                .OrderBy(prioridade => prioridade.ID)
                .Select(prioridade => new { prioridade.ID, prioridade.Nome })
                .ToListAsync();

            return Ok(prioridades);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Criar() => Forbid();

        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Atualizar(int id) => Forbid();

        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Excluir(int id) => Forbid();
    }
}
