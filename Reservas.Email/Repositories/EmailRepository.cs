using Azure.Communication.Email;
using Reservas.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reservas.Email.Repositories
{
    public class EmailRepository : IEmailRepository
    {
        private readonly EmailClient _emailClient;

        public EmailRepository(EmailClient emailClient)
        {
            this._emailClient = emailClient;
        }

        public  Task SendEmailAsync(string to, string subject, string body)
        {
            var sender = "DoNotReply@04ad303e-77c8-4887-a472-1b65606694e2.azurecomm.net";
            var emailContent = new EmailContent(subject)
            {
                PlainText = body,
                
            };
            var emailMessage = new EmailMessage(sender,to, emailContent);
            _emailClient.Send(Azure.WaitUntil.Started, emailMessage);

           return Task.CompletedTask;
        }
    }
}
