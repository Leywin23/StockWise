using System.Net;
using System.Net.Mail;

namespace StockWise.Application.Interfaces
{
    public interface IEmailSenderService
    {
        Task SendEmailAsync(string to, string subject, string body, CancellationToken ct = default);

    }
}
