//using MimeKit;
//using MimeKit.Text;
//using MailKit.Net.Smtp;
//using MailKit.Security;

//public class EmailService
//{
//    private readonly string _smtpServer = "smtp.ethereal.email";
//    private readonly int _smtpPort = 587;
//    private readonly string _smtpUser = "alvah.gleason@ethereal.email";
//    private readonly string _smtpPass = "bgRBW5TpvawzpKZXvc";

//    public void SendEmail(string to, string subject, string body)
//    {
//        var email = new MimeMessage();
//        email.From.Add(MailboxAddress.Parse(_smtpUser));
//        email.To.Add(MailboxAddress.Parse(to));
//        email.Subject = subject;
//        email.Body = new TextPart(TextFormat.Html) { Text = body };

//        using var smtp = new SmtpClient();
//        smtp.Connect(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
//        smtp.Authenticate(_smtpUser, _smtpPass);
//        smtp.Send(email);
//        smtp.Disconnect(true);
//    }
//}

using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit.Text;
using MimeKit;
using MailKit.Net.Smtp;


public class EmailSettings
{
    public string SmtpServer { get; set; }
    public int SmtpPort { get; set; }
    public string SmtpUser { get; set; }
    public string SmtpPass { get; set; }
    public string FromEmail { get; set; }
}

public class EmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public void SendEmail(string to, string subject, string body)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(_settings.FromEmail));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;
        email.Body = new TextPart(TextFormat.Html) { Text = body };

        using var smtp = new SmtpClient();
        smtp.Connect(_settings.SmtpServer, _settings.SmtpPort, SecureSocketOptions.StartTls);
        smtp.Authenticate(_settings.SmtpUser, _settings.SmtpPass);
        smtp.Send(email);
        smtp.Disconnect(true);
    }
}

