using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace TaskGX.API.Services
{
    public class EmailSender
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IOptions<EmailSettings> settings, ILogger<EmailSender> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
        {
            // validações básicas (evita NullReference e erros bobos)
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("toEmail é obrigatório.", nameof(toEmail));

            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException("subject é obrigatório.", nameof(subject));

            if (string.IsNullOrWhiteSpace(htmlBody))
                throw new ArgumentException("htmlBody é obrigatório.", nameof(htmlBody));

            if (string.IsNullOrWhiteSpace(_settings.Host))
                throw new InvalidOperationException("EmailSettings.Host não configurado.");

            if (_settings.Port <= 0)
                throw new InvalidOperationException("EmailSettings.Port inválido.");

            if (string.IsNullOrWhiteSpace(_settings.Username))
                throw new InvalidOperationException("EmailSettings.Username não configurado.");

            if (string.IsNullOrWhiteSpace(_settings.Password))
                throw new InvalidOperationException("EmailSettings.Password não configurado.");

            // Se FromEmail não vier, usamos Username como fallback
            var fromEmail = string.IsNullOrWhiteSpace(_settings.FromEmail) ? _settings.Username : _settings.FromEmail;

            using var message = new MailMessage
            {
                From = new MailAddress(fromEmail, _settings.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(new MailAddress(toEmail));

            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                Timeout = 10000
            };

            try
            {
                ct.ThrowIfCancellationRequested();
                await client.SendMailAsync(message);
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "Falha SMTP ao enviar email para {ToEmail}. Host={Host} Port={Port}", toEmail, _settings.Host, _settings.Port);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao enviar email para {ToEmail}", toEmail);
                throw;
            }
        }

        // atalho útil pra email de verificação
        public Task SendVerificationCodeAsync(string toEmail, string code, DateTime? expiresAt = null, CancellationToken ct = default)
        {
            var exp = expiresAt is null ? "" : $"<p>Expira em: <b>{expiresAt:dd/MM/yyyy HH:mm}</b></p>";

            var subject = "Seu código de verificação - TaskGX";
            var body = $@"
                <div style='font-family: Arial, sans-serif; line-height:1.5'>
                    <h2>TaskGX</h2>
                    <p>Seu código de verificação é:</p>
                    <p style='font-size: 22px; letter-spacing: 2px;'><b>{WebUtility.HtmlEncode(code)}</b></p>
                    {exp}
                    <p>Se você não solicitou isso, pode ignorar este email.</p>
                </div>";

            return SendAsync(toEmail, subject, body, ct);
        }
    }
}
