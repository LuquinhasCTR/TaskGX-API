using System.ComponentModel.DataAnnotations;

namespace TaskGX.API.DTOs
{
    public class SolicitarAlteracaoEmailRequest
    {
        [Required(ErrorMessage = "O novo email e obrigatorio.")]
        [EmailAddress(ErrorMessage = "O novo email informado e invalido.")]
        [StringLength(150, ErrorMessage = "O novo email deve ter no maximo 150 caracteres.")]
        public string NovoEmail { get; set; } = string.Empty;
    }
}
