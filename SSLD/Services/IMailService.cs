namespace SSLD.Services;

public interface IMailService
{
    Task<int> SendForgetPasswordMail();
    Task SendEmailAsync(string email, string subject, string htmlMessage);
}