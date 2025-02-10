using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using OurHeritage.Core.Entities;
using OurHeritage.Service.Interfaces;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace OurHeritage.Service.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly UserManager<User> _userManager;
        private readonly IOptions<DataProtectionTokenProviderOptions> _tokenProviderOptions;

        public EmailService(IConfiguration config, UserManager<User> userManager,
            IOptions<DataProtectionTokenProviderOptions> tokenProviderOptions
            )
        {
            _config = config;
            _userManager = userManager;
            _tokenProviderOptions = tokenProviderOptions;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config["EmailSettings:Email"]));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_config["EmailSettings:Host"], int.Parse(_config["EmailSettings:Port"]!),
                MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_config["EmailSettings:Email"], _config["EmailSettings:Password"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

    }
}
