using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using QRCodeAttendance.Interface.Services;
using QRCodeAttendance.Models.Configuration;

namespace QRCodeAttendance.Implementation.Services
{
    public class ResendEmailService : IEmailService
    {
        private const string SendEmailUrl = "https://api.resend.com/emails";
        private readonly HttpClient _httpClient;
        private readonly ResendOptions _options;

        public ResendEmailService(HttpClient httpClient, IOptions<ResendOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task SendInstructorInvitationCodeAsync(string instructorEmail, string code, DateTime expiryDate)
        {
            if (string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                throw new InvalidOperationException("Resend API key is not configured.");
            }

            if (string.IsNullOrWhiteSpace(_options.SenderEmail))
            {
                throw new InvalidOperationException("Resend sender email is not configured.");
            }

            var sender = string.IsNullOrWhiteSpace(_options.SenderName)
                ? _options.SenderEmail
                : $"{_options.SenderName} <{_options.SenderEmail}>";

            var requestBody = new
            {
                from = sender,
                to = new[] { instructorEmail },
                subject = "Your MITC Instructor Registration Code",
                html = BuildHtmlContent(code, expiryDate, _options.LogoUrl),
                text = BuildTextContent(code, expiryDate)
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, SendEmailUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

            using var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var detail = string.IsNullOrWhiteSpace(responseBody)
                ? response.ReasonPhrase
                : responseBody;

            throw new InvalidOperationException($"Resend could not deliver the email ({(int)response.StatusCode}): {detail}");
        }

        private static string BuildHtmlContent(string code, DateTime expiryDate, string logoUrl)
        {
            var logo = string.IsNullOrWhiteSpace(logoUrl)
                ? string.Empty
                : $"<img src=\"{logoUrl}\" alt=\"MITC logo\" width=\"96\" style=\"display:block;width:96px;max-width:96px;height:auto;margin:0 auto 14px;border:0;\" />";

            return $@"<!doctype html>
<html lang=""en""><body style=""margin:0;padding:32px;background:#edf4f3;font-family:Arial,Helvetica,sans-serif;color:#132f3f;"">
  <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0""><tr><td align=""center"">
    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""max-width:620px;background:#fff;border:1px solid #d7e6e4;border-radius:16px;"">
      <tr><td style=""padding:24px 28px 22px;text-align:center;background:#0f766e;color:#fff;"">{logo}<strong style=""font-size:16px;letter-spacing:0.5px;"">MITC QR Code Attendance</strong></td></tr>
      <tr><td style=""padding:34px;text-align:center;""><h1 style=""margin:0 0 14px;"">Your instructor access code</h1>
        <p>Use this code to complete your instructor registration.</p>
        <p style=""display:inline-block;padding:16px 24px;background:#e7fbfa;border:1px solid #bcefeb;border-radius:12px;color:#0f766e;font-size:26px;font-weight:700;letter-spacing:4px;"">{code}</p>
        <p style=""color:#536879;"">This code is linked to your email address and expires in 24 hours.</p>
      </td></tr>
    </table>
  </td></tr></table>
</body></html>";
        }

        private static string BuildTextContent(string code, DateTime expiryDate)
        {
            return $"Welcome to MITC QRCode Attendance System. Your instructor registration code is {code}. Please use it within 24 hours.";
        }
    }
}
