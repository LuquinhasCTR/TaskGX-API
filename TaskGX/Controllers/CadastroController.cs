using Microsoft.AspNetCore.Mvc;
using TaskGX.API.DTOs;
using TaskGX.API.Services;

namespace TaskGX.API.Controllers
{
    [ApiController]
    [Route("api/cadastro")]
    public class CadastroController : ControllerBase
    {
        private readonly CadastroService _cadastroService;

        public CadastroController(CadastroService cadastroService)
        {
            _cadastroService = cadastroService;
        }

        [HttpPost]
        public async Task<IActionResult> Cadastrar([FromBody] CadastroDTO dtoCadastro)
        {
            var resultado = await _cadastroService.CriarContaAsync(dtoCadastro);
            if (!resultado.Sucesso)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Nao foi possivel concluir o cadastro.",
                    Detail = resultado.Mensagem,
                    Status = StatusCodes.Status400BadRequest
                });
            }

            return Ok(new { mensagem = resultado.Mensagem });
        }
    }
}
