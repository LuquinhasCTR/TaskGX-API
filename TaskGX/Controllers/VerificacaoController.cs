using Microsoft.AspNetCore.Mvc;
using TaskGX.API.DTOs;
using TaskGX.API.Services;

namespace TaskGX.API.Controllers
{
    [ApiController]
    [Route("api/verificacao")]
    public class VerificacaoController : ControllerBase
    {
        private readonly VerificacaoService _verificacaoService;

        public VerificacaoController(VerificacaoService verificacaoService)
        {
            _verificacaoService = verificacaoService;
        }

        [HttpPost("verificar-email")]
        public async Task<IActionResult> VerificarEmail([FromBody] VerificacaoDTO dtoVerificacao)
        {
            var resultado = await _verificacaoService.VerificarEmailAsync(dtoVerificacao.Email, dtoVerificacao.Codigo);
            if (!resultado.Sucesso)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Falha na verificacao de email.",
                    Detail = resultado.Mensagem,
                    Status = StatusCodes.Status400BadRequest
                });
            }

            return Ok(new { mensagem = resultado.Mensagem });
        }

        [HttpPost("reenviar-codigo")]
        public async Task<IActionResult> ReenviarCodigo([FromBody] ReenviarCodigoDTO dtoReenvio)
        {
            var resultado = await _verificacaoService.ReenviarCodigoAsync(dtoReenvio.Email);
            if (!resultado.Sucesso)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Falha ao reenviar o codigo.",
                    Detail = resultado.Mensagem,
                    Status = StatusCodes.Status400BadRequest
                });
            }

            return Ok(new { mensagem = resultado.Mensagem });
        }
    }
}
