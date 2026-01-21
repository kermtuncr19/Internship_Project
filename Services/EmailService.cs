using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace StoreApp.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            var smtpServer = _configuration["Email:SmtpServer"];
            var smtpPortRaw = _configuration["Email:SmtpPort"];
            var senderEmail = _configuration["Email:SenderEmail"];
            var senderName = _configuration["Email:SenderName"];
            var username = _configuration["Email:Username"];
            var password = _configuration["Email:Password"];

            if (string.IsNullOrWhiteSpace(smtpServer))
                throw new InvalidOperationException("Email:SmtpServer boş/null. Railway env varlarını kontrol et (Email__SmtpServer).");

            if (string.IsNullOrWhiteSpace(senderEmail))
                throw new InvalidOperationException("Email:SenderEmail boş/null.");

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException("Email:Username/Email:Password boş/null.");

            var smtpPort = int.TryParse(smtpPortRaw, out var p) ? p : 587;

            // 🔥 Railway’de pending kalmasın diye kritik:
            const int timeoutMs = 10000; // 10 sn

            _logger.LogInformation("SMTP send starting. Server={Server} Port={Port} User={User} To={To}",
                smtpServer, smtpPort, username, to);

            try
            {
                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(username, password),
                    Timeout = timeoutMs,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(to);

                // SendMailAsync bazen Timeout’u tam uygulamıyor; garanti olsun diye Task.WhenAny ile sarıyoruz:
                var sendTask = client.SendMailAsync(mailMessage);
                var completed = await Task.WhenAny(sendTask, Task.Delay(timeoutMs));

                if (completed != sendTask)
                    throw new TimeoutException($"SMTP gönderimi {timeoutMs}ms içinde tamamlanmadı (Server={smtpServer}, Port={smtpPort}).");

                await sendTask;

                _logger.LogInformation("E-posta başarıyla gönderildi: {To}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "E-posta gönderilirken hata oluştu: {To} (Server={Server}, Port={Port})",
                    to, smtpServer, smtpPort);
                throw;
            }
        }
    }
}
