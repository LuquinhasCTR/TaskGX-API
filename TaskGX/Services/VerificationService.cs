using System;
using System.Threading.Tasks;
using TaskGX.API.Repositories;
using TaskGX.Repositories;

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
            var usuario = await _usuarioRepository.ObterPorEmailAsync(email);
            if (usuario == null) return (false, "Email não encontrado.");

            if (usuario.EmailVerificado) return (true, "Email já verificado.");
            if (usuario.CodigoVerificacao != codigo) return (false, "Código inválido.");
            if (usuario.CodigoVerificacaoExpiracao < DateTime.UtcNow) return (false, "Código expirado.");

            await _usuarioRepository.AtualizarVerificacaoEmailAsync(usuario.ID, true, true, null, null);
            return (true, "Email verificado com sucesso.");
        }

        public async Task<(bool Sucesso, string Mensagem)> ReenviarCodigoAsync(string email)
        {
            var usuario = await _usuarioRepository.ObterPorEmailAsync(email);
            if (usuario == null) return (false, "Email não encontrado.");

            var codigo = RegistrationService.GerarCodigoVerificacao();
            await _usuarioRepository.AtualizarVerificacaoEmailAsync(usuario.ID, false, false, codigo, DateTime.UtcNow.AddHours(24));

            try
            {
                await _emailSender.SendVerificationCodeAsync(email, codigo, expiresAt: usuario.CodigoVerificacaoExpiracao);
            }
            catch
            {
                return (false, "Não foi possível reenviar o código no momento.");
            }

            return (true, "Novo código enviado com sucesso.");
        }
    }
}
