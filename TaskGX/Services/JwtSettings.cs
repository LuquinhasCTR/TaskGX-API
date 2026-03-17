using System.ComponentModel.DataAnnotations;

namespace TaskGX.API.Services
{
    public class JwtSettings
    {
        [Required]
        [MinLength(32)]
        public string Key { get; set; } = string.Empty;

        [Required]
        public string Issuer { get; set; } = "TaskGX";

        [Required]
        public string Audience { get; set; } = "TaskGX";

        [Range(1, 1440)]
        public int ExpireMinutes { get; set; } = 480;
    }
}
