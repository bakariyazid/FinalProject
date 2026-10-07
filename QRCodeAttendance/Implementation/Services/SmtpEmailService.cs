using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using QRCodeAttendance.Interface.Services;
using QRCodeAttendance.Models.Configuration;

namespace QRCodeAttendance.Implementation.Services
{
  public class SmtpEmailService : IEmailService
  {
    private const string LogoContentId = "mitc-logo";
    private readonly SmtpOptions _options;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public SmtpEmailService(IOptions<SmtpOptions> options, IWebHostEnvironment webHostEnvironment)
    {
      _options = options.Value;
      _webHostEnvironment = webHostEnvironment;
    }

    public async Task SendInstructorInvitationCodeAsync(string instructorEmail, string code, DateTime expiryDate)
    {
      EnsureSmtpConfigured();

      using var message = new MailMessage
      {
        From = new MailAddress(_options.SenderEmail, _options.SenderName),
        Subject = "Your MITC Instructor Registration Code",
        SubjectEncoding = Encoding.UTF8
      };
      message.To.Add(instructorEmail);

      var logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "mitc-logo.jpg");
      var hasEmbeddedLogo = File.Exists(logoPath);
      var logoSource = hasEmbeddedLogo ? $"cid:{LogoContentId}" : _options.LogoUrl;

      message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(
          BuildTextContent(code, expiryDate),
          Encoding.UTF8,
          "text/plain"));

      var htmlView = AlternateView.CreateAlternateViewFromString(
          BuildHtmlContent(code, expiryDate, logoSource),
          Encoding.UTF8,
          "text/html");

      if (hasEmbeddedLogo)
      {
        var embeddedLogo = new LinkedResource(logoPath, MediaTypeNames.Image.Jpeg)
        {
          ContentId = LogoContentId,
          TransferEncoding = TransferEncoding.Base64
        };
        htmlView.LinkedResources.Add(embeddedLogo);
      }

      message.AlternateViews.Add(htmlView);

      using var smtpClient = CreateSmtpClient();
      await smtpClient.SendMailAsync(message);
    }

    public async Task SendInstructorEmailVerificationCodeAsync(string instructorEmail, string code, DateTime expiryDate)
    {
      EnsureSmtpConfigured();

      // Validate recipient format before opening SMTP connection
      try
      {
        _ = new MailAddress(instructorEmail);
      }
      catch
      {
        throw new InvalidOperationException("The instructor email address is not valid.");
      }

      using var message = new MailMessage
      {
        From = new MailAddress(_options.SenderEmail, _options.SenderName),
        Subject = "Confirm your MITC instructor email",
        SubjectEncoding = Encoding.UTF8
      };
      message.To.Add(instructorEmail);

      var logoPath = Path.Combine(_webHostEnvironment.WebRootPath ?? string.Empty, "images", "mitc-logo.jpg");
      var hasEmbeddedLogo = !string.IsNullOrWhiteSpace(_webHostEnvironment.WebRootPath) && File.Exists(logoPath);
      var logoSource = hasEmbeddedLogo ? $"cid:{LogoContentId}" : (_options.LogoUrl ?? string.Empty);

      message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(
          BuildVerificationTextContent(code, expiryDate),
          Encoding.UTF8,
          "text/plain"));

      var htmlView = AlternateView.CreateAlternateViewFromString(
          BuildVerificationHtmlContent(code, expiryDate, logoSource),
          Encoding.UTF8,
          "text/html");

      if (hasEmbeddedLogo)
      {
        var embeddedLogo = new LinkedResource(logoPath, MediaTypeNames.Image.Jpeg)
        {
          ContentId = LogoContentId,
          TransferEncoding = TransferEncoding.Base64
        };
        htmlView.LinkedResources.Add(embeddedLogo);
      }

      message.AlternateViews.Add(htmlView);

      using var smtpClient = CreateSmtpClient();
      await smtpClient.SendMailAsync(message);
    }

    private void EnsureSmtpConfigured()
    {
      if (string.IsNullOrWhiteSpace(_options.Host))
        throw new InvalidOperationException("SMTP host is not configured. Set Smtp:Host in appsettings.");
      if (_options.Port <= 0)
        throw new InvalidOperationException("SMTP port is not configured. Use 587 for Gmail.");
      if (string.IsNullOrWhiteSpace(_options.SenderEmail))
        throw new InvalidOperationException("SMTP sender email is not configured. Set Smtp:SenderEmail in appsettings.");
      if (string.IsNullOrWhiteSpace(_options.Username) || string.IsNullOrWhiteSpace(_options.Password))
        throw new InvalidOperationException("SMTP username/password are not configured. For Gmail, use your full email and an App Password (not your normal password).");
    }

    private SmtpClient CreateSmtpClient()
    {
      return new SmtpClient(_options.Host, _options.Port)
      {
        EnableSsl = _options.EnableSsl,
        UseDefaultCredentials = false,
        Credentials = new NetworkCredential(_options.Username, _options.Password),
        DeliveryMethod = SmtpDeliveryMethod.Network,
        Timeout = 30000
      };
    }

    internal static string BuildVerificationHtmlContent(string code, DateTime expiryDate, string logoSource) => $@"<!doctype html><html><body style=""font-family:Arial,sans-serif;color:#132f3f""><h2>Confirm your email address</h2><p>An MITC administrator is preparing an instructor registration invitation for this email address.</p><p>Share this code with the administrator only if you requested this invitation:</p><p style=""font-size:28px;font-weight:bold;letter-spacing:6px;color:#0f766e"">{code}</p><p>This code expires at {expiryDate.ToLocalTime():h:mm tt} and can be used once.</p><p>If you did not expect this, safely ignore this email.</p></body></html>";

    internal static string BuildVerificationTextContent(string code, DateTime expiryDate) => $"MITC email verification code: {code}. Share it with the administrator only if you requested an instructor invitation. It expires at {expiryDate.ToLocalTime():h:mm tt}.";

    internal static string BuildHtmlContent(string code, DateTime expiryDate, string logoSource)
    {
      var logo = string.IsNullOrWhiteSpace(logoSource)
          ? string.Empty
          : $@"<img src=""{logoSource}"" alt=""MITC logo"" width=""150"" style=""display:block;margin:0 auto;max-width:150px;height:auto;border:0;"" />";

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
            <td align=""center"" style=""padding:26px 30px 24px;background:linear-gradient(135deg,#075a57 0%,#0f766e 58%,#12958b 100%);"">
              <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" style=""margin:0 auto;"">
                <tr>
                  <td align=""center"" style=""width:170px;height:130px;padding:14px;border-radius:24px;background:#ffffff;box-shadow:0 8px 22px rgba(0,0,0,0.18);"">
                    {logo}
                  </td>
                </tr>
              </table>
              <p style=""margin:16px 0 0;color:#d9f8f5;font-size:12px;font-weight:700;letter-spacing:1.6px;text-transform:uppercase;"">MITC QR Code Attendance</p>
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
                      This code is linked to your email address only and expires after 
                      <strong style=""color:#223849;"">24 hours</strong>.
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

    internal static string BuildTextContent(string code, DateTime expiryDate)
    {
      return $"Welcome to MITC QRCode Attendance System. Your instructor registration code is {code}. Please use it as soon as possible. It is linked to this email address only and expires after 24 hours.";
    }
  }
}
