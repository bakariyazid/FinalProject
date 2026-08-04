using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using QRCodeAttendance.Interface.Services;
using QRCodeAttendance.Models.Configuration;

namespace QRCodeAttendance.Implementation.Services
{
    public class BrevoEmailService : IEmailService
    {
        private const string SendEmailUrl = "https://api.brevo.com/v3/smtp/email";
        private readonly HttpClient _httpClient;
        private readonly BrevoOptions _options;

        public BrevoEmailService(HttpClient httpClient, IOptions<BrevoOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task SendInstructorInvitationCodeAsync(string instructorEmail, string code, DateTime expiryDate)
        {
            if (string.IsNullOrWhiteSpace(_options.ApiKey))
                throw new InvalidOperationException("Brevo API key is not configured.");

            if (string.IsNullOrWhiteSpace(_options.SenderEmail))
                throw new InvalidOperationException("Brevo sender email is not configured.");

            var requestBody = new
            {
                sender = new { email = _options.SenderEmail, name = _options.SenderName },
                to = new[] { new { email = instructorEmail } },
                subject = "Your MITC Instructor Registration Code",
                htmlContent = SmtpEmailService.BuildHtmlContent(code, expiryDate, _options.LogoUrl),
                textContent = SmtpEmailService.BuildTextContent(code, expiryDate)
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, SendEmailUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("api-key", _options.ApiKey);

            using var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
                return;

            var responseBody = await response.Content.ReadAsStringAsync();
            var detail = string.IsNullOrWhiteSpace(responseBody) ? response.ReasonPhrase : responseBody;
            throw new InvalidOperationException($"Brevo could not deliver the email ({(int)response.StatusCode}): {detail}");
        }
    }
}
