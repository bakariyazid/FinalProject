using System.Net.Mail;
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

        /// <summary>
        /// First email: 6-digit OTP so admin can confirm the instructor mailbox is real.
        /// </summary>
        public async Task SendInstructorEmailVerificationCodeAsync(string instructorEmail, string code, DateTime expiryDate)
        {
            await SendEmailAsync(
                instructorEmail,
                "Confirm your MITC instructor email",
                SmtpEmailService.BuildVerificationHtmlContent(code, expiryDate, _options.LogoUrl ?? string.Empty),
                SmtpEmailService.BuildVerificationTextContent(code, expiryDate));
        }

        /// <summary>
        /// Second email: registration code after OTP verification succeeds.
        /// </summary>
        public async Task SendInstructorInvitationCodeAsync(string instructorEmail, string code, DateTime expiryDate)
        {
            await SendEmailAsync(
                instructorEmail,
                "Your MITC Instructor Registration Code",
                SmtpEmailService.BuildHtmlContent(code, expiryDate, _options.LogoUrl ?? string.Empty),
                SmtpEmailService.BuildTextContent(code, expiryDate));
        }

        private async Task SendEmailAsync(string instructorEmail, string subject, string htmlContent, string textContent)
        {
            if (string.IsNullOrWhiteSpace(_options.ApiKey))
                throw new InvalidOperationException("Brevo API key is not configured. Set Brevo:ApiKey in appsettings.");

            if (string.IsNullOrWhiteSpace(_options.SenderEmail))
                throw new InvalidOperationException("Brevo sender email is not configured. Set Brevo:SenderEmail in appsettings.");

            // Validate recipient before calling Brevo
            try
            {
                _ = new MailAddress(instructorEmail);
            }
            catch
            {
                throw new InvalidOperationException("The instructor email address is not valid.");
            }

            var requestBody = new
            {
                sender = new
                {
                    email = _options.SenderEmail.Trim(),
                    name = string.IsNullOrWhiteSpace(_options.SenderName)
                        ? "MITC QRCode Attendance"
                        : _options.SenderName.Trim()
                },
                to = new[] { new { email = instructorEmail.Trim() } },
                subject,
                htmlContent,
                textContent
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, SendEmailUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            request.Headers.TryAddWithoutValidation("api-key", _options.ApiKey.Trim());
            request.Headers.TryAddWithoutValidation("accept", "application/json");

            using var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
                return;

            var responseBody = await response.Content.ReadAsStringAsync();
            var detail = string.IsNullOrWhiteSpace(responseBody) ? response.ReasonPhrase : responseBody;
            throw new InvalidOperationException($"Brevo could not deliver the email ({(int)response.StatusCode}): {detail}");
        }
    }
}
