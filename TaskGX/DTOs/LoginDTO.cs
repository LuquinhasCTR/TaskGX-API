using System.ComponentModel.DataAnnotations;

namespace TaskGX.API.DTOs
{
    public class LoginDTO
    {
        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(128, MinimumLength = 8)]
        public string Senha { get; set; } = string.Empty;
    }
}
