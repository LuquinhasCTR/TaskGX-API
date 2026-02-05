using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskGX.Data;
using TaskGX.API.DTOs;

namespace TaskGX.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrioridadesController : ControllerBase
    {
        private readonly TaskGXContext _context;

        public PrioridadesController(TaskGXContext context)
        {
            _context = context;
        }

        // GET: api/prioridades
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PrioridadeDTO>>> GetPrioridades()
        {
            var prioridades = await _context.Prioridades
                .Select(p => new PrioridadeDTO
                {
                    ID = p.ID,
                    Nome = p.Nome
                })
                .ToListAsync();

            return prioridades;
        }
    }
}
