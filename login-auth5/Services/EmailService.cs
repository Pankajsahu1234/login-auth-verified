using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace login_auth5.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendOtpEmailAsync(string toEmail, string otp)
        {
            var emailSettings = _config.GetSection("EmailSettings");

            var senderName = emailSettings["SenderName"] ?? "OTP Service";
            var senderEmail = emailSettings["SenderEmail"] ?? throw new InvalidOperationException("SenderEmail is not configured.");
            var smtpServer = emailSettings["SmtpServer"] ?? throw new InvalidOperationException("SmtpServer is not configured.");
            var smtpPort = int.TryParse(emailSettings["Port"], out var port) ? port : 587;
            var username = emailSettings["Username"] ?? throw new InvalidOperationException("SMTP Username is not configured.");
            var password = emailSettings["Password"] ?? throw new InvalidOperationException("SMTP Password is not configured.");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "Your OTP Code";

            message.Body = new TextPart("plain")
            {
                Text = $"Your OTP code is: {otp}"
            };

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(username, password);
                await client.SendAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailService] Failed to send OTP email: {ex.Message}");
                throw;
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}