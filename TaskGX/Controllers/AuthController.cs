using Microsoft.AspNetCore.Mvc;
using TaskGX.API.DTOs;
using TaskGX.API.Services;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly TokenService _tokenService;

    public AuthController(AuthService authService, TokenService tokenService)
    {
        _authService = authService;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto)
    {
        var usuario = await _authService.LoginAsync(dto.Email, dto.Senha);

        // aqui o AuthService deve retornar null se:
        // - credenciais inválidas
        // - usuário não ativo
        // - email não verificado (recomendado)
        if (usuario == null)
            return Unauthorized("Credenciais inválidas ou usuário não autorizado.");

        var token = _tokenService.CreateToken(usuario);

        return Ok(new
        {
            token,
            usuario = new { usuario.ID, usuario.Nome, usuario.Email }
        });
    }
}
