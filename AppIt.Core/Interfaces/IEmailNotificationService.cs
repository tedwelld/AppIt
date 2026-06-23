namespace AppIt.Core.Interfaces.Services
{
    public interface IEmailNotificationService
    {
        Task SendAsync(string to, string subject, string body);
    }
}
