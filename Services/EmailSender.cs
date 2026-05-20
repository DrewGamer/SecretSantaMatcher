using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace SecretSantaMatcher.Services
{
    public class EmailSender
    {
        public const string SmtpHost = "smtp.gmail.com";
        public const int SmtpPort = 587;

        public static async Task TestConnectionAsync(string senderEmail, string appPassword)
        {
            if (string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(appPassword))
            {
                throw new ArgumentException("Sender email and Gmail App Password must be provided.");
            }

            using (var mail = new MailMessage())
            {
                mail.From = new MailAddress(senderEmail, "Secret Santa Matcher");
                mail.To.Add(new MailAddress(senderEmail));
                mail.Subject = "Secret Santa Matcher - SMTP Connection Test";
                mail.Body = "<h1>Success!</h1><p>Your Gmail SMTP connection was configured correctly. You are ready to match and send emails!</p>";
                mail.IsBodyHtml = true;

                using (var smtp = new SmtpClient(SmtpHost, SmtpPort))
                {
                    smtp.Credentials = new NetworkCredential(senderEmail, appPassword);
                    smtp.EnableSsl = true;
                    // Timeout set to 10 seconds for snappy responses on error
                    smtp.Timeout = 10000;
                    await smtp.SendMailAsync(mail);
                }
            }
        }

        public static async Task SendEmailAsync(
            string senderEmail, 
            string appPassword, 
            string organizerName,
            string recipientEmail, 
            string recipientName, 
            string subjectTemplate, 
            string bodyTemplate,
            string matchedName, 
            string matchedWishlist)
        {
            if (string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(appPassword))
            {
                throw new ArgumentException("Sender email and Gmail App Password must be provided.");
            }

            // Replace template placeholder tokens
            string subject = ReplaceTokens(subjectTemplate, organizerName, recipientName, matchedName, matchedWishlist);
            string body = ReplaceTokens(bodyTemplate, organizerName, recipientName, matchedName, matchedWishlist);

            using (var mail = new MailMessage())
            {
                mail.From = new MailAddress(senderEmail, string.IsNullOrWhiteSpace(organizerName) ? "Secret Santa Organizer" : organizerName);
                mail.To.Add(new MailAddress(recipientEmail, recipientName));
                mail.Subject = subject;
                mail.Body = body;
                // Auto-detect if body contains HTML tags to render properly
                mail.IsBodyHtml = body.Trim().StartsWith("<") || body.Contains("<html>") || body.Contains("<p>") || body.Contains("<br>");

                using (var smtp = new SmtpClient(SmtpHost, SmtpPort))
                {
                    smtp.Credentials = new NetworkCredential(senderEmail, appPassword);
                    smtp.EnableSsl = true;
                    smtp.Timeout = 15000; // 15 seconds
                    await smtp.SendMailAsync(mail);
                }
            }
        }

        public static string ReplaceTokens(string template, string organizer, string giver, string receiver, string wishlist)
        {
            if (string.IsNullOrEmpty(template)) return string.Empty;

            return template
                .Replace("{Organizer}", organizer ?? "")
                .Replace("{Giver}", giver ?? "")
                .Replace("{Receiver}", receiver ?? "")
                .Replace("{Wishlist}", wishlist ?? "");
        }
    }
}
