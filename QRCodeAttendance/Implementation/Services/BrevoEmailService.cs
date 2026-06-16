using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using QRCodeAttendance.Interface.Services;
using QRCodeAttendance.Models.Configuration;

namespace QRCodeAttendance.Implementation.Services
{
    public class BrevoEmailService : IEmailService
    {
        private const string BrevoSendEmailUrl = "https://api.brevo.com/v3/smtp/email";

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
            {
                throw new InvalidOperationException("Brevo API key is not configured.");
            }

            if (string.IsNullOrWhiteSpace(_options.SenderEmail))
            {
                throw new InvalidOperationException("Brevo sender email is not configured.");
            }

            var htmlContent = BuildHtmlContent(code, expiryDate);
            var textContent = BuildTextContent(code, expiryDate);

            using var request = new HttpRequestMessage(HttpMethod.Post, BrevoSendEmailUrl);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("api-key", _options.ApiKey);

            var payload = new
            {
                sender = new
                {
                    name = _options.SenderName,
                    email = _options.SenderEmail
                },
                to = new[]
                {
                    new
                    {
                        email = instructorEmail
                    }
                },
                subject = "Your MITC Instructor Registration Code",
                htmlContent,
                textContent
            };

            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Brevo could not send the instructor code. Status: {(int)response.StatusCode}. Response: {error}");
            }
        }

        private string BuildHtmlContent(string code, DateTime expiryDate)
        {
            var logo = string.IsNullOrWhiteSpace(_options.LogoUrl)
                ? string.Empty
                : $@"<img src=""{_options.LogoUrl}"" alt=""MITC logo"" width=""92"" style=""display:block;margin:0 auto;max-width:92px;height:auto;border:0;"" />";

            var expiry = expiryDate.ToLocalTime().ToString("MMM dd, yyyy h:mm tt");

            return $@"
<!doctype html>
<html lang=""en"">
<head>
  <meta charset=""utf-8"">
  <meta name=""viewport"" content=""width=device-width,initial-scale=1"">
  <meta name=""x-apple-disable-message-reformatting"">
  <title>Your MITC Instructor Registration Code</title>
</head>
<body style=""margin:0;padding:0;background:#edf4f3;font-family:Arial,Helvetica,sans-serif;color:#132f3f;"">
  <div style=""display:none;max-height:0;overflow:hidden;opacity:0;color:transparent;"">
    Your secure MITC instructor registration code is ready.
  </div>
  <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""background:#edf4f3;padding:34px 14px;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""max-width:620px;background:#ffffff;border-radius:16px;overflow:hidden;border:1px solid #d7e6e4;box-shadow:0 18px 42px rgba(19,47,63,0.12);"">
          <tr>
            <td style=""height:7px;background:#0f766e;font-size:0;line-height:0;"">&nbsp;</td>
          </tr>
          <tr>
            <td style=""padding:30px 30px 10px;text-align:center;background:#ffffff;"">
              {logo}
            </td>
          </tr>
          <tr>
            <td style=""padding:14px 34px 36px;text-align:center;"">
              <p style=""margin:0 0 10px;color:#0f766e;font-size:12px;font-weight:700;letter-spacing:1.4px;text-transform:uppercase;"">Instructor Registration</p>
              <h1 style=""margin:0 0 14px;font-size:27px;line-height:1.25;color:#112533;font-weight:800;"">Your MITC access code is ready</h1>
              <p style=""margin:0 auto 26px;max-width:480px;font-size:15px;line-height:1.7;color:#536879;"">
                Use this secure code to complete your instructor registration for the MITC QRCode Attendance System.
              </p>
              <table role=""presentation"" align=""center"" cellspacing=""0"" cellpadding=""0"" style=""margin:0 auto 24px;"">
                <tr>
                  <td style=""padding:17px 28px;border-radius:12px;background:#e7fbfa;border:1px solid #bcefeb;color:#0f766e;font-size:26px;font-weight:800;letter-spacing:4px;text-align:center;box-shadow:inset 0 0 0 1px rgba(15,118,110,0.05);"">
                    {code}
                  </td>
                </tr>
              </table>
              <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""background:#f7faf9;border:1px solid #dfe9e7;border-radius:12px;"">
                <tr>
                  <td style=""padding:18px 20px;text-align:left;"">
                    <p style=""margin:0 0 7px;font-size:13px;font-weight:700;color:#112533;"">Code details</p>
                    <p style=""margin:0;font-size:13px;line-height:1.7;color:#5d7080;"">
                      This code is linked to your email address only and expires on
                      <strong style=""color:#223849;"">{expiry}</strong>.
                    </p>
                  </td>
                </tr>
              </table>
              <p style=""margin:22px 0 0;font-size:12px;line-height:1.6;color:#8495a3;"">
                If you did not expect this invitation, you can safely ignore this email.
              </p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
        }

        private static string BuildTextContent(string code, DateTime expiryDate)
        {
            return $"Welcome to MITC QRCode Attendance System. Your instructor registration code is {code}. Please use it as soon as possible. It is linked to this email address only and expires on {expiryDate.ToLocalTime():MMM dd, yyyy h:mm tt}.";
        }
    }
}
