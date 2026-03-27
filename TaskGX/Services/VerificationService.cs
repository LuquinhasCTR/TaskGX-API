using TaskGX.API.Repositories;

namespace TaskGX.API.Services
{
    public class VerificationService
    {
        private readonly UsuarioRepository _usuarioRepository;
        private readonly EmailSender _emailSender;

        public VerificationService(UsuarioRepository usuarioRepository, EmailSender emailSender)
        {
            _usuarioRepository = usuarioRepository;
            _emailSender = emailSender;
        }

        public async Task<(bool Sucesso, string Mensagem)> VerificarEmailAsync(string email, string codigo)
        {
            email = (email ?? string.Empty).Trim().ToLowerInvariant();
            codigo = (codigo ?? string.Empty).Trim();

            var usuario = await _usuarioRepository.ObterPorEmailAsync(email);
            if (usuario == null) return (false, "Email nao encontrado.");

            if (usuario.EmailVerificado) return (true, "Email ja verificado.");
            if (usuario.CodigoVerificacao != codigo) return (false, "Codigo invalido.");
            if (usuario.CodigoVerificacaoExpiracao is null || usuario.CodigoVerificacaoExpiracao < DateTime.UtcNow)
                return (false, "Codigo expirado.");

            await _usuarioRepository.AtualizarVerificacaoEmailAsync(usuario.ID, true, true, null, null);
            return (true, "Email verificado com sucesso.");
        }

        public async Task<(bool Sucesso, string Mensagem)> ReenviarCodigoAsync(string email)
        {
            email = (email ?? string.Empty).Trim().ToLowerInvariant();

            var usuario = await _usuarioRepository.ObterPorEmailAsync(email);
            if (usuario == null) return (false, "Email nao encontrado.");

            if (usuario.EmailVerificado) return (true, "Email ja verificado.");

            var codigo = RegistrationService.GerarCodigoVerificacao();
            var expiracao = DateTime.UtcNow.AddHours(24);

            await _usuarioRepository.AtualizarVerificacaoEmailAsync(usuario.ID, false, false, codigo, expiracao);

            try
            {
                await _emailSender.SendVerificationCodeAsync(email, codigo, expiresAt: expiracao);
            }
            catch
            {
                return (false, "Nao foi possivel reenviar o codigo no momento.");
            }

            return (true, "Novo codigo enviado com sucesso.");
        }
    }
}
