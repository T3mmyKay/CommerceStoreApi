using SendGrid;
using SendGrid.Helpers.Mail;

namespace T3mmyStoreApi.Services
{
    public class EmailSender
    {
        private readonly IConfiguration _configuration;
        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task SendEmail(string subject, string toEmail, string userName, string message)
        {
            var apiKey = _configuration["EmailSender:ApiKey"]!;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_configuration["EmailSender:FromEmail"]!, _configuration["EmailSender:SenderName"]!);
            var to = new EmailAddress(toEmail, userName);
            var plainTextContent = "";
            var htmlContent = message;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }
    }
}
