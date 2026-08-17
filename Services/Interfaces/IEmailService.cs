namespace Pos.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task<bool> SendPasswordResetEmailAsync(string to, string userName, string resetToken);
        Task<bool> SendPasswordChangedNotificationAsync(string to, string userName);
    }
}