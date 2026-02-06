using TaskGX.API.Models;
using TaskGX.API.Repositories;

namespace TaskGX.API.Services
{
    public class AuthService
    {
        private readonly UsuarioRepository _usuarioRepository;

        public AuthService(UsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<Usuarios?> LoginAsync(string email, string senhaDigitada)
        {
            email = (email ?? "").Trim().ToLowerInvariant();

            var usuario = await _usuarioRepository.ObterPorEmailAsync(email);
            if (usuario == null) return null;

            if (!PasswordService.Verify(senhaDigitada, usuario.SenhaHash))
                return null;

            if (!usuario.Ativo || !usuario.EmailVerificado)
                return null;

            return usuario;
        }
    }
}
