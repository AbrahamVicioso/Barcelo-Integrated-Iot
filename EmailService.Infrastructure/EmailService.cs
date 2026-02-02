using Azure.Communication.Email;
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
        private readonly EmailClient emailClient;

        public EmailService(SmtpSettings smtpSettings, ILogger<EmailService> logger, EmailClient emailClient)
        {
            _smtpSettings = smtpSettings;
            _logger = logger;
            this.emailClient = emailClient;
        }

        public async Task<bool> SendEmailAsync(EmailNotification notification, CancellationToken cancellationToken = default)
        {
            try
            {
                var message = new EmailMessage(
                    senderAddress: _smtpSettings.Username,
                    recipientAddress: notification.To,
                    content: new EmailContent(notification.Subject)
                    {
                        Html = notification.Body
                });

                await emailClient.SendAsync(Azure.WaitUntil.Started, message);
                
                _logger.LogInformation("Email sent successfully to {Recipient}", notification.To);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Recipient}", notification.To);
                return false;
            }
        }

    }

    public class SmtpSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
    }
}
