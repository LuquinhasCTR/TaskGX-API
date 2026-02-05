using Microsoft.AspNetCore.Mvc;
using TaskGX.API.DTOs;
using TaskGX.API.Services;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto)
    {
        var usuario = await _authService.LoginAsync(dto.Email, dto.Senha);
        if (usuario == null) return Unauthorized("Credenciais inválidas ou usuário não ativo.");
        return Ok(new { usuario.ID, usuario.Nome, usuario.Email });
    }
}
