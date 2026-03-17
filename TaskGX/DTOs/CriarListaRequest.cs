using System.ComponentModel.DataAnnotations;

namespace TaskGX.API.DTOs
{
    public class CriarListaRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Cor { get; set; }

        public bool Favorita { get; set; }
    }
}
