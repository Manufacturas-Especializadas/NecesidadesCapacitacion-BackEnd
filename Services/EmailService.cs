using Microsoft.Extensions.Options;
using NecesidadesCapacitacion.Data;
using NecesidadesCapacitacion.Models;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;

namespace NecesidadesCapacitacion.Services
{
    public class EmailService
    {
        public EmailSettings Setting { get; }

        public EmailService(IOptions<EmailSettings> options)
        {
            Setting = options.Value;
        }

        public async Task SendEmailAsync(IEnumerable<string> toEmails, string subject, string body)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(Setting.SenderEmail, Setting.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
                Priority = MailPriority.High,
            };

            foreach (var email in toEmails)
            {
                if (!string.IsNullOrWhiteSpace(email))
                {
                    mailMessage.To.Add(email);
                }
            }

            using var client = new SmtpClient
            {
                Host = Setting.Host,
                Port = Setting.Port,
                EnableSsl = Setting.UseSSL,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(Setting.Username, Setting.Password)
            };

            await client.SendMailAsync(mailMessage);
        }        
    }
}