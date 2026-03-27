using TaskGX.API.DTOs;
using TaskGX.API.Models;
using TaskGX.API.Repositories;

namespace TaskGX.API.Services
{
    public class CadastroService
    {
        private readonly UsuarioRepository _usuarioRepository;
        private readonly EnvioEmailService _envioEmailService;
        private readonly ILogger<CadastroService> _registrador;

        public CadastroService(
            UsuarioRepository usuarioRepository,
            EnvioEmailService envioEmailService,
            ILogger<CadastroService> registrador)
        {
            _usuarioRepository = usuarioRepository;
            _envioEmailService = envioEmailService;
            _registrador = registrador;
        }

        public Task<(bool Sucesso, string Mensagem)> CriarContaAsync(CadastroDTO dtoCadastro)
            => CriarContaAsync(dtoCadastro.Nome, dtoCadastro.Email, dtoCadastro.Senha, dtoCadastro.ConfirmarSenha);

        public async Task<(bool Sucesso, string Mensagem)> CriarContaAsync(
            string nome,
            string email,
            string senha,
            string confirmarSenha)
        {
            nome = (nome ?? string.Empty).Trim();
            email = (email ?? string.Empty).Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(nome) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(senha) ||
                string.IsNullOrWhiteSpace(confirmarSenha))
            {
                return (false, "Todos os campos sao obrigatorios.");
            }

            if (!SenhaService.EhValida(senha))
                return (false, "Senha nao atende aos requisitos de seguranca.");

            if (senha != confirmarSenha)
                return (false, "As senhas nao coincidem.");

            if (await _usuarioRepository.ExisteEmailAsync(email))
                return (false, "Email ja cadastrado.");

            var usuario = new Usuario
            {
                Nome = nome,
                Email = email,
                SenhaHash = SenhaService.GerarHash(senha),
                Ativo = true,
                EmailVerificado = false,
                CodigoVerificacao = GerarCodigoVerificacao(),
                CodigoVerificacaoExpiracao = DateTime.UtcNow.AddHours(24),
                CriadoEm = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow
            };

            await _usuarioRepository.InserirAsync(usuario);

            try
            {
                await _envioEmailService.EnviarCodigoVerificacaoAsync(
                    emailDestino: usuario.Email,
                    codigo: usuario.CodigoVerificacao!,
                    expiresAt: usuario.CodigoVerificacaoExpiracao);

                return (true, "Conta criada com sucesso. Verifique seu email.");
            }
            catch (Exception ex)
            {
                _registrador.LogError(ex, "Falha ao enviar email de verificacao para {Email}", usuario.Email);
                return (true, "Conta criada, mas nao foi possivel enviar o email de verificacao. Use 'Reenviar codigo'.");
            }
        }

        internal static string GerarCodigoVerificacao()
        {
            var codigo = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, 1_000_000);
            return codigo.ToString("D6");
        }
    }
}
