using System.ComponentModel.DataAnnotations;

namespace TaskGX.API.DTOs
{
    public class AtualizarUsuarioRequest
    {
        [Required(ErrorMessage = "O nome e obrigatorio.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "O avatar deve ter no maximo 255 caracteres.")]
        public string? Avatar { get; set; }
    }
}
