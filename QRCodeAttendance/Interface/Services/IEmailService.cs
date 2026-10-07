namespace QRCodeAttendance.Interface.Services
{
    public interface IEmailService
    {
        Task SendInstructorEmailVerificationCodeAsync(string instructorEmail, string code, DateTime expiryDate);
        Task SendInstructorInvitationCodeAsync(string instructorEmail, string code, DateTime expiryDate);
    }
}
