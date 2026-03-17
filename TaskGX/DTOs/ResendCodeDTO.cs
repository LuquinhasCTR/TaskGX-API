using System.ComponentModel.DataAnnotations;

namespace TaskGX.API.DTOs
{
    public class ResendCodeDTO
    {
        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;
    }
}
