namespace QRCodeAttendance.Interface.Services
{
    public interface IEmailService
    {
        Task SendInstructorInvitationCodeAsync(string instructorEmail, string code, DateTime expiryDate);
    }
}
