using System.Net;
using StockWise.Interfaces;
using StockWise.Helpers;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;
using System.Text.RegularExpressions;

namespace StockWise.Services
{
    public class EmailSenderService : IEmailSenderServicer
    {
        private readonly EmailSettings _settings;

        public EmailSenderService(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.From));
            message.To.Add(MailboxAddress.Parse(to));

            message.Subject = subject;

            var builder = new BodyBuilder {
                TextBody = StripHtml(body),
                HtmlBody = body
            };
            message.Body = builder.ToMessageBody();

            var client = new SmtpClient
            {
                Timeout = 10000
            };

            await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);

            var appPassword = (_settings.Password ?? string.Empty);
            await client.AuthenticateAsync(_settings.User, appPassword);
            client.SendAsync(message);



        }

        private static string StripHtml(string html) {
            if(string.IsNullOrEmpty(html)) return string.Empty;

            var noScriptsStyles = Regex.Replace(
                 html,
            "<(script|style)[^>]*?>.*?</\\1>",
            string.Empty,
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

            var noTags = Regex.Replace(
            noScriptsStyles,
            "<[^>]+>",
            " ",
            RegexOptions.Singleline);
            var normalized = Regex.Replace(noTags, "\\s{2,}", " ").Trim();

            return WebUtility.HtmlDecode(normalized);
        }
    }
}
