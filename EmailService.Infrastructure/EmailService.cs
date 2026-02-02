using Microsoft.Extensions.Logging;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Notification.Email
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(SmtpSettings smtpSettings, ILogger<EmailService> logger)
        {
            _smtpSettings = smtpSettings;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(EmailNotification notification, CancellationToken cancellationToken = default)
        {
            try
            {
                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.FromName),
                    Subject = notification.Subject,
                    Body = notification.Body,
                    IsBodyHtml = notification.IsHtml
                };

                mailMessage.To.Add(new MailAddress(notification.To));

                using var smtpClient = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
                {
                    Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                    EnableSsl = _smtpSettings.EnableSsl
                };

                await smtpClient.SendMailAsync(mailMessage, cancellationToken);
                
                _logger.LogInformation("Email sent successfully to {Recipient}", notification.To);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Recipient}", notification.To);
                return false;
            }
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false, CancellationToken cancellationToken = default)
        {
            var notification = new EmailNotification
            {
                To = to,
                Subject = subject,
                Body = body,
                IsHtml = isHtml
            };

            return await SendEmailAsync(notification, cancellationToken);
        }
    }

    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
    }
}
