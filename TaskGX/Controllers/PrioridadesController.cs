using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskGX.API.Models;
using TaskGX.Data;

namespace TaskGX.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PrioridadesController : ControllerBase
    {
        private readonly TaskGXContext _context;

        public PrioridadesController(TaskGXContext context)
        {
            _context = context;
        }

        // GET: api/prioridades
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var prioridades = await _context.Prioridades
                .OrderBy(p => p.ID)
                .Select(p => new { p.ID, p.Nome })
                .ToListAsync();

            return Ok(prioridades);
        }

        // Se ainda não tem roles/admin: trava alterações por segurança
        [HttpPost]
        [Authorize] // mantém, mas vamos bloquear mesmo assim
        public IActionResult Post() => Forbid();

        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Put(int id) => Forbid();

        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(int id) => Forbid();
    }
}
