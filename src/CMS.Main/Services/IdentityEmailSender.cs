using System.Net;
using CMS.Main.Data;
using CMS.Main.Emails.Templates;
using FluentEmail.Core;
using Microsoft.AspNetCore.Identity;

namespace CMS.Main.Services;

public class IdentityEmailSender(
    ILogger<IdentityEmailSender> logger, 
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
            logger.LogError(ex, "There was an error when sending confirmation email to {userId}.", user.Id);
        }
    }

    public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        try
        {
            var result = await fluentEmail
                .To(email)
                .Subject("Reset your password")
                .UsingTemplateFromFile("Emails/Templates/ResetPasswordLink.cshtml",
                    new ResetPasswordLinkModel { ResetLink = WebUtility.HtmlDecode(resetLink) })
                .SendAsync();

            if (!result.Successful)
            {
                logger.LogError("There was an error when sending password reset link email to {userId}.", user.Id);
                return;
            }
            
            logger.LogInformation("Password reset link email sent to {userId}.", user.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There was an error when sending password reset link email to {userId}.", user.Id);
        }
    }

    public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        try
        {
            var result = await fluentEmail
                .To(email)
                .Subject("Reset your password")
                .UsingTemplateFromFile("Emails/Templates/ResetPasswordCode.cshtml",
                    new ResetPasswordCodeModel { ResetCode = resetCode})
                .SendAsync();

            if (!result.Successful)
            {
                logger.LogError("There was an error when sending password reset code email to {userId}.", user.Id);
                return;
            }
            
            logger.LogInformation("Password reset code email sent to {userId}.", user.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There was an error when sending password reset code email to {userId}.", user.Id);
        }
    }
}