using System.ComponentModel.DataAnnotations;

namespace TaskGX.API.DTOs
{
    public class AlterarSenhaRequest
    {
        [Required]
        [StringLength(128, MinimumLength = 8)]
        public string SenhaAtual { get; set; } = string.Empty;

        [Required]
        [StringLength(128, MinimumLength = 8)]
        public string NovaSenha { get; set; } = string.Empty;

        [Required]
        [StringLength(128, MinimumLength = 8)]
        public string ConfirmarNovaSenha { get; set; } = string.Empty;
    }
}
