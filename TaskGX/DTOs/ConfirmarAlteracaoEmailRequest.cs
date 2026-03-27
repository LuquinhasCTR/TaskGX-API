using System.ComponentModel.DataAnnotations;

namespace TaskGX.API.DTOs
{
    public class ConfirmarAlteracaoEmailRequest
    {
        [Required(ErrorMessage = "O codigo de confirmacao e obrigatorio.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "O codigo deve conter 6 digitos.")]
        public string Codigo { get; set; } = string.Empty;
    }
}
