using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using StockWise.Application.Interfaces;

namespace StockWise.Infrastructure.Services
{
    public sealed class EmailSenderService : IEmailSenderService
    {
        private readonly EmailSettings _settings;

        public EmailSenderService(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_settings.SmtpServer))
                throw new InvalidOperationException("EmailSettings.SmtpServer is empty – check appsettings.");
            if (_settings.SmtpPort <= 0)
                throw new InvalidOperationException("EmailSettings.SmtpPort must be > 0.");
            if (string.IsNullOrWhiteSpace(_settings.SenderEmail))
                throw new InvalidOperationException("EmailSettings.SenderEmail is empty – check appsettings.");

            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(_settings.SenderName ?? string.Empty, _settings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject ?? string.Empty;

            var builder = new BodyBuilder
            {
                HtmlBody = body ?? string.Empty,
                TextBody = StripHtml(body)
            };
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient { Timeout = 10000 };

            await client.ConnectAsync(
                _settings.SmtpServer,
                _settings.SmtpPort,
                _settings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto,
                ct);

            if (!string.IsNullOrWhiteSpace(_settings.Username))
            {
                await client.AuthenticateAsync(_settings.Username, _settings.Password ?? string.Empty, ct);
            }

            await client.SendAsync(message, ct);

            await client.DisconnectAsync(true, ct);
        }

        private static string StripHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return string.Empty;

            var noScriptsStyles = Regex.Replace(
                html,
                "<(script|style)[^>]*?>.*?</\\1>",
                string.Empty,
                RegexOptions.Singleline | RegexOptions.IgnoreCase);

            var noTags = Regex.Replace(noScriptsStyles, "<[^>]+>", " ", RegexOptions.Singleline);
            var normalized = Regex.Replace(noTags, "\\s{2,}", " ").Trim();

            return WebUtility.HtmlDecode(normalized);
        }
    }
}
