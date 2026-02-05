using System;
using System.Threading.Tasks;
using TaskGX.API.DTOs;
using TaskGX.API.Models;
using TaskGX.API.Repositories;

namespace TaskGX.API.Services
{
    public class RegistrationService
    {
        private readonly UsuarioRepository _usuarioRepository;
        private readonly EmailSender _emailSender;
        private readonly ILogger<RegistrationService> _logger;

        public RegistrationService(
            UsuarioRepository usuarioRepository,
            EmailSender emailSender,
            ILogger<RegistrationService> logger)
        {
            _usuarioRepository = usuarioRepository;
            _emailSender = emailSender;
            _logger = logger;
        }

        public Task<(bool Sucesso, string Mensagem)> CriarContaAsync(RegistrationDTO dto)
            => CriarContaAsync(dto.Nome, dto.Email, dto.Senha, dto.ConfirmarSenha);

        public async Task<(bool Sucesso, string Mensagem)> CriarContaAsync(
            string nome, string email, string senha, string confirmarSenha)
        {
            // Normalização básica
            nome = (nome ?? "").Trim();
            email = (email ?? "").Trim().ToLowerInvariant();

            // Validação
            if (string.IsNullOrWhiteSpace(nome) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(senha) ||
                string.IsNullOrWhiteSpace(confirmarSenha))
                return (false, "Todos os campos são obrigatórios.");

            if (!PasswordService.IsValid(senha))
                return (false, "Senha não atende aos requisitos de segurança.");

            if (senha != confirmarSenha)
                return (false, "As senhas não coincidem.");

            if (await _usuarioRepository.ExisteEmailAsync(email))
                return (false, "Email já cadastrado.");

            // Criar usuário
            var usuario = new Usuarios
            {
                Nome = nome,
                Email = email,
                SenhaHash = PasswordService.Hash(senha),

                Ativo = true,
                EmailVerificado = false,

                CodigoVerificacao = GerarCodigoVerificacao(),
                CodigoVerificacaoExpiracao = DateTime.UtcNow.AddHours(24),

                CriadoEm = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow
            };

            await _usuarioRepository.InserirAsync(usuario);

            // Enviar email
            try
            {
                await _emailSender.SendVerificationCodeAsync(
                    toEmail: usuario.Email,
                    code: usuario.CodigoVerificacao!,
                    expiresAt: usuario.CodigoVerificacaoExpiracao
                );

                return (true, "Conta criada com sucesso! Verifique seu email.");
            }
            catch (Exception ex)
            {
                // ✅ NÃO engole: loga o motivo real
                _logger.LogError(ex, "Falha ao enviar email de verificação para {Email}", usuario.Email);

                // Mantém a conta criada (igual você queria), mas orienta o usuário a usar /resend-code
                return (true, "Conta criada, mas não foi possível enviar o email de verificação. Use 'Reenviar código'.");
            }
        }

        internal static string GerarCodigoVerificacao()
        {
            var codigo = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, 1_000_000);
            return codigo.ToString("D6");
        }
    }
}
