using System.Net.Mail;
using System.Net;
using StockWise.Interfaces;
using StockWise.Helpers;
using Microsoft.Extensions.Options;

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
            var from = new MailAddress("stockwiseappilication@gmail.com", "StockWise App");
            var toAddr = new MailAddress(to);
            var password = _settings.Password;

            using var smtp = new SmtpClient
            {
                Host = _settings.Host,
                Port = _settings.Port,
                EnableSsl = _settings.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_settings.User, password)
            };

            using var msg = new MailMessage(from, toAddr)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            await smtp.SendMailAsync(msg);
        }
    }
}
