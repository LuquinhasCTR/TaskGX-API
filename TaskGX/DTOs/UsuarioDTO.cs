namespace TaskGX.API.DTOs
{
    public class UsuarioDTO
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public bool Ativo { get; set; }
        public bool EmailVerificado { get; set; }
    }
}
