using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Net;
using Ticketing.Application.Interfaces.Services;

namespace Ticketing.Infrastructure.Email;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var host = _configuration["Email:Host"];
        var port = _configuration.GetValue<int?>("Email:Port") ?? 587;
        var username = _configuration["Email:Username"];
        var password = _configuration["Email:Password"];
        var fromName = _configuration["Email:FromName"] ?? "ISTS";
        var fromEmail = _configuration["Email:FromEmail"] ?? "no-reply@ists.local";

        // In development with localhost, just log instead of sending
        if (string.IsNullOrEmpty(host) || host == "localhost")
        {
            _logger.LogInformation("Development mode: Email not sent. To={To}, Subject={Subject}", to, subject);
            return;
        }

        try
        {
            using var client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(username, password),
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage, cancellationToken);
            _logger.LogInformation("Email sent to {To} with subject {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To} with subject {Subject}", to, subject);
            throw;
        }
    }
}