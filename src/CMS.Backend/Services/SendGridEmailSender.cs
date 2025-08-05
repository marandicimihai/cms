using Ardalis.Result;
using CMS.Backend.Abstractions;
using CMS.Backend.Commands;
using CMS.Backend.Data;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace CMS.Backend.Services;

public class SendGridEmailSender : ISendGridEmailSender
{
    private readonly ILogger<SendGridEmailSender> logger;
    private readonly SendGridClient? client;
    
    public SendGridEmailSender(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        logger = loggerFactory.CreateLogger<SendGridEmailSender>();
        
        var apiKey = configuration["SendGrid:ApiKey"];
        if (apiKey is null)
        {
            logger.LogWarning("SendGrid API key not found. Email sending will not work.");
        }
        else
        {
            client = new(apiKey);
        }
    }

    public async Task<Result> SendEmailAsync(string userId, SendGridMessage message)
    {
        if (client is null) 
            return Result.Invalid();

        try
        {
            var response = await client!.SendEmailAsync(message);

            if (response.IsSuccessStatusCode) return Result.Success();
            
            logger.LogError("Error sending email to {userId}.", userId);
            return Result.Error($"There was an error sending the email to {userId}.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending email to {userId}.", userId);
            return Result.Error($"There was an error sending the email to {userId}.");
        }
    }
}