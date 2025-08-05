using CMS.Backend.Commands;
using CMS.Backend.Data;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using SendGrid.Helpers.Mail;

namespace CMS.Backend.Services;

public class IdentityEmailQueuer : IEmailSender<ApplicationUser>
{
    private readonly ILogger<IdentityEmailQueuer> logger;
    private readonly EmailSenderOptions? options;

    public IdentityEmailQueuer(ILogger<IdentityEmailQueuer> logger, IConfiguration configuration)
    {
        this.logger = logger;
        
        options = configuration.GetSection("SendGrid").Get<EmailSenderOptions>();
        if (options is null)
        {
            logger.LogWarning("SendGrid options not found. Email sending will not work.");
        }
    }
    
    public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        if (options is null) return;
        
        var from = new EmailAddress(options.From, options.FromName);
        var to = new EmailAddress(email);
        
        var msg = MailHelper.CreateSingleTemplateEmail(from, to, options.ConfirmationEmailTemplateId, new
        {
            confirmationLink = "asdasd"
        });
        
        await new SendEmailCommand
        {
            UserId = user.Id,
            Message = msg
        }.QueueJobAsync();
        
        logger.LogInformation("Confirmation email queued for {userId}.", user.Id);
    }

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        throw new NotImplementedException();
    }

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        throw new NotImplementedException();
    }
    
    internal sealed class EmailSenderOptions
    {
        public string From { get; set; } = default!;
        public string FromName { get; set; } = default!;
        public string ConfirmationEmailTemplateId { get; set; } = default!;
    }
}