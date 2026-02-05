using Microsoft.AspNetCore.Mvc;
using TaskGX.API.DTOs;
using TaskGX.API.Services;

[ApiController]
[Route("api/register")]
public class RegistrationController : ControllerBase
{
    private readonly RegistrationService _registrationService;

    public RegistrationController(RegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegistrationDTO dto)
    {
        var resultado = await _registrationService.CriarContaAsync(dto);
        if (!resultado.Sucesso) return BadRequest(resultado.Mensagem);
        return Ok(resultado.Mensagem);
    }
}
