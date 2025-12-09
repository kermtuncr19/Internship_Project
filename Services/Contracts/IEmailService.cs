// Services/IEmailService.cs
namespace StoreApp.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlBody);
    }
}