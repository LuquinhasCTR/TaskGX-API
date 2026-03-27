using System.ComponentModel.DataAnnotations;

namespace TaskGX.API.Services
{
    public class ConfiguracoesJwt
    {
        [Required]
        [MinLength(32)]
        public string Chave { get; set; } = string.Empty;

        [Required]
        public string Emissor { get; set; } = "TaskGX";

        [Required]
        public string Audiencia { get; set; } = "TaskGX";

        [Range(1, 1440)]
        public int MinutosExpiracao { get; set; } = 480;
    }
}
