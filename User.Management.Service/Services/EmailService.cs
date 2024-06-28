using MimeKit;
using System.Net.Mail;
using User.Management.Service.Models;


namespace User.Management.Service.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailConfiguartion _emailConfig;

        private static Dictionary<string, (string code, DateTime expiry)> userCodes = new Dictionary<string, (string, DateTime)>();
        private const int CodeValidityDurationMinutes = 5;


        public EmailService(EmailConfiguartion emailConfig) => _emailConfig = emailConfig;

        public void SendEmail(Message message)
        {
            var emailMessage = CreateEmailMessage(message);
            Send(emailMessage);
           StoreCode(message.To.Select(addr => addr.Address).ToList(), message.Content);
           Console.WriteLine($"Stored{userCodes} ");


        }

        private MimeMessage CreateEmailMessage(Message message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("email", _emailConfig.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = message.Content };

            return emailMessage;
        }
       
        private void Send (MimeMessage mailMessage)
        {
            using var client = new MailKit.Net.Smtp.SmtpClient();
            try
            {
                client.Connect(_emailConfig.SmtpServer, _emailConfig.Port, true);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(_emailConfig.UserName, _emailConfig.Password);

                client.Send(mailMessage);


            }
            catch
            {
                throw;
            }
            finally
            {
                client.Disconnect(true);
                client.Dispose();
            }
        }

         public void StoreCode(List<string> emailAddresses, string code)
         {
             foreach (var emailAddress in emailAddresses)
             {
                 userCodes[emailAddress] = (code, DateTime.Now.AddMinutes(CodeValidityDurationMinutes));
             }

         }

         public async Task<bool> CheckEmailVerificationCode(string email, string otp)
         {
             if (userCodes.TryGetValue(email, out var storedCode))
             {
                 return storedCode.code == otp && DateTime.Now <= storedCode.expiry;
             }
             return false;
         }
        
    }
}
