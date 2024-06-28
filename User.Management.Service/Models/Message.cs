using MimeKit;

namespace User.Management.Service.Models
{
    public class Message
    {
        public List<MailboxAddress> To { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }

        public Message(IEnumerable<string> to, string subject, string content)
        {
            To = new List<MailboxAddress>();
            To.AddRange(to.Select(x => new MailboxAddress("email", x)));
            Subject = subject;
            Content = content;
        }
        
        public Message(IEnumerable<string> to, string subject)
        {
            To = new List<MailboxAddress>();
            To.AddRange(to.Select(x => new MailboxAddress("email", x)));
            Subject = subject;
            Content = GenerateCode();
        }
        
        
        private string GenerateCode()
        {
            var random = new Random();
            var code = new char[4];
            for (int i = 0; i < code.Length; i++)
            {
                code[i] = (char)('A' + random.Next(0, 26)); // Generates a random uppercase letter
            }
            return new string(code);
        }
        
    }
}
