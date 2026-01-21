using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace StoreApp.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _http;

        public EmailService(IConfiguration config, IHttpClientFactory factory)
        {
            _config = config;
            _http = factory.CreateClient();
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            var apiKey = _config["BREVO_API_KEY"];
            var senderEmail = _config["BREVO_SENDER_EMAIL"];
            var senderName = _config["BREVO_SENDER_NAME"];

            var payload = new
            {
                sender = new { email = senderEmail, name = senderName },
                to = new[] { new { email = to } },
                subject = subject,
                htmlContent = htmlBody
            };

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.brevo.com/v3/smtp/email"
            );

            request.Headers.Add("api-key", apiKey);
            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );

            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Brevo mail error: {error}");
            }
        }
    }
}
