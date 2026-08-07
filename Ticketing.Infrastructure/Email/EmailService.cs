using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using Ticketing.Application.Interfaces.Services;

namespace Ticketing.Infrastructure.Email;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly HttpClient _httpClient;

    public EmailService(
        IConfiguration configuration,
        ILogger<EmailService> logger,
        HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task SendAsync(
        string to,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["Email:ApiKey"];
        var fromEmail = _configuration["Email:FromEmail"];
        var fromName = _configuration["Email:FromName"] ?? "ISTS";

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException(
                "Email API key is not configured.");

        if (string.IsNullOrWhiteSpace(fromEmail))
            throw new InvalidOperationException(
                "Sender email is not configured.");

        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.brevo.com/v3/smtp/email"
            );

            request.Headers.Add("api-key", apiKey);

            request.Content = JsonContent.Create(new
            {
                sender = new
                {
                    name = fromName,
                    email = fromEmail
                },

                to = new[]
                {
                    new
                    {
                        email = to
                    }
                },

                subject = subject,

                htmlContent = htmlBody
            });

            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(
                    cancellationToken
                );

                throw new InvalidOperationException(
                    $"Brevo email failed: {(int)response.StatusCode} {error}"
                );
            }

            _logger.LogInformation(
                "Email sent successfully to {To} with subject {Subject}",
                to,
                subject
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send email to {To} with subject {Subject}",
                to,
                subject
            );

            throw;
        }
    }
}