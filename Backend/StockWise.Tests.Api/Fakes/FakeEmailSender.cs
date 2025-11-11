using Microsoft.AspNetCore.Identity.UI.Services;
using StockWise.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Tests.Api.Fakes
{
    public sealed class FakeEmailSender : IEmailSenderService
    {
        public Task SendEmailAsync(string to, string subject, string body, CancellationToken ct = default)
            =>Task.CompletedTask;
        
    }
}
