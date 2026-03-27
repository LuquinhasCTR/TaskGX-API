using TaskGX.API.Models;
using TaskGX.API.Repositories;

namespace TaskGX.API.Services
{
    public class AutenticacaoService
    {
        private readonly UsuarioRepository _usuarioRepository;

        public AutenticacaoService(UsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<Usuario?> AutenticarAsync(string email, string senhaInformada)
        {
            email = (email ?? string.Empty).Trim().ToLowerInvariant();

            var usuario = await _usuarioRepository.ObterPorEmailAsync(email);
            if (usuario == null)
                return null;

            if (!SenhaService.Verificar(senhaInformada, usuario.SenhaHash))
                return null;

            if (!usuario.Ativo || !usuario.EmailVerificado)
                return null;

            return usuario;
        }
    }
}
