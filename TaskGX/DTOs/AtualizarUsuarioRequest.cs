using System.ComponentModel.DataAnnotations;

namespace TaskGX.API.DTOs
{
    public class AtualizarUsuarioRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Avatar { get; set; }
    }
}
