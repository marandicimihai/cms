using Ardalis.Result;
using SendGrid.Helpers.Mail;

namespace CMS.Backend.Abstractions;

public interface ISendGridEmailSender
{
    Task<Result> SendEmailAsync(string userId, SendGridMessage message);
}