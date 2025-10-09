using System.Net;
using System.Net.Mail;

namespace StockWise.Interfaces
{
    public interface IEmailSenderServicer
    {
        Task SendEmailAsync(string to, string subject, string body, CancellationToken ct = default);

    }
}
