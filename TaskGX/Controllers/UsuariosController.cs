using Microsoft.AspNetCore.Mvc;
using TaskGX.API.Models;
using TaskGX.Data;  
using TaskGX.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace TaskGX.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly TaskGXContext _context;

        public UsuariosController(TaskGXContext context)
        {
            _context = context;
        }

        // GET: api/usuarios/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioDTO>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null) return NotFound();

            var usuarioDTO = new UsuarioDTO
            {
                Nome = usuario.Nome,
                Email = usuario.Email,
                Avatar = usuario.Avatar
            };

            return usuarioDTO;
        }

        // POST: api/usuarios
        [HttpPost]
        public async Task<ActionResult<UsuarioDTO>> CriarUsuario(Usuarios usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            var usuarioDTO = new UsuarioDTO
            {
                Nome = usuario.Nome,
                Email = usuario.Email,
                Avatar = usuario.Avatar
            };

            return CreatedAtAction(nameof(GetUsuario), new { id = usuario.ID }, usuarioDTO);
        }
    }
}
