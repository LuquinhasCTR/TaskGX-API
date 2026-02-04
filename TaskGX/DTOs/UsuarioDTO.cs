namespace TaskGX.API.DTOs
{
    public class UsuarioDTO
    {
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Avatar { get; set; }
        public bool Ativo { get; set; }
        public bool EmailVerificado { get; set; }
    }
}
