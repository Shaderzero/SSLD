using System.Net;
using System.Net.Mail;
using System.Text;

namespace SSLD.Services;

public class MailService : IMailService
{
    private readonly string _from;
    private readonly string _siteAddress;
    private readonly string _host;
    private readonly int _port;

    private readonly SmtpClient _client;

    public MailService(IConfiguration configuration)
    {
        _from = configuration.GetValue<string>("Email:From");
        _siteAddress = configuration.GetValue<string>("SiteAddress");
        _host = configuration.GetValue<string>("Email:Smtp:Host");
        _port = configuration.GetValue<int>("Email:Smtp:Port");
        _client = CreateClient();
    }

    private SmtpClient CreateClient()
    {
        var client = new SmtpClient();
        var nc = CredentialCache.DefaultNetworkCredentials;
        client.Host = _host;
        client.Port = _port;
        client.UseDefaultCredentials = true;
        client.Credentials = (System.Net.ICredentialsByHost) nc.GetCredential(
            _host,
            _port,
            "Basic");
        return client;
    }

    public Task<int> SendForgetPasswordMail()
    {
        // throw new NotImplementedException();
        return null;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            using var smtpClient = _client;
            var mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(_from);
            mailMessage.To.Add(new MailAddress(email));
            mailMessage.IsBodyHtml = true;
            mailMessage.SubjectEncoding = Encoding.UTF8;
            mailMessage.BodyEncoding = Encoding.UTF8;
            mailMessage.Subject = subject;
            mailMessage.Body = htmlMessage;

            await smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}