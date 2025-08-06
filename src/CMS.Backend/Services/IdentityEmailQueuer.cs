using System.Net;
using CMS.Backend.Data;
using CMS.Backend.Emails.Templates;
using FluentEmail.Core;
using Microsoft.AspNetCore.Identity;

namespace CMS.Backend.Services;

public class IdentityEmailQueuer(
    ILogger<IdentityEmailQueuer> logger, 
    IFluentEmail fluentEmail
) : IEmailSender<ApplicationUser>
{
    public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        try
        {
            var result = await fluentEmail
                .To(email)
                .Subject("Confirm your account")
                .UsingTemplateFromFile("Emails/Templates/ConfirmAccount.cshtml",
                    new ConfirmAccountModel { ConfirmationLink = WebUtility.HtmlDecode(confirmationLink) })
                .SendAsync();

            if (!result.Successful)
            {
                logger.LogError("There was an error when sending confirmation email to {userId}.", user.Id);
                return;
            }
            
            logger.LogInformation("Confirmation email sent to {userId}.", user.Id);
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "There was an error when sending confirmation email to {userId}.", user.Id);
        }
    }

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        throw new NotImplementedException();
    }

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        throw new NotImplementedException();
    }
}