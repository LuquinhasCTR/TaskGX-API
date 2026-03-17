using Microsoft.AspNetCore.Mvc;
using TaskGX.API.DTOs;
using TaskGX.API.Services;

[ApiController]
[Route("api/verification")]
public class VerificationController : ControllerBase
{
    private readonly VerificationService _verificationService;

    public VerificationController(VerificationService verificationService)
    {
        _verificationService = verificationService;
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerificationDTO dto)
    {
        var resultado = await _verificationService.VerificarEmailAsync(dto.Email, dto.Codigo);
        if (!resultado.Sucesso)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Falha na verificação de email.",
                Detail = resultado.Mensagem,
                Status = StatusCodes.Status400BadRequest
            });
        }

        return Ok(new { message = resultado.Mensagem });
    }

    [HttpPost("resend-code")]
    public async Task<IActionResult> ResendCode([FromBody] ResendCodeDTO dto)
    {
        var resultado = await _verificationService.ReenviarCodigoAsync(dto.Email);
        if (!resultado.Sucesso)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Falha ao reenviar o código.",
                Detail = resultado.Mensagem,
                Status = StatusCodes.Status400BadRequest
            });
        }

        return Ok(new { message = resultado.Mensagem });
    }
}
