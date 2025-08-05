using CMS.Backend.Abstractions;
using FastEndpoints;
using SendGrid.Helpers.Mail;

namespace CMS.Backend.Commands;

internal sealed class SendEmailCommand : ICommand
{
    public required string UserId { get; init; }
    public required SendGridMessage Message { get; init; }
}

internal sealed class SendEmailCommandHandler(IServiceScopeFactory serviceScopeFactory)
    : ICommandHandler<SendEmailCommand>
{
    public async Task ExecuteAsync(SendEmailCommand command, CancellationToken ct)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var emailSender = scope.ServiceProvider.GetRequiredService<ISendGridEmailSender>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<SendEmailCommandHandler>>();
        
        var result = await emailSender.SendEmailAsync(command.UserId, command.Message);

        if (result.IsSuccess)
        {
            logger.LogInformation("Sent email to user {userId}.", command.UserId);
        }
        else
        {
            logger.LogError("There was an error when sending email to user {userId}.", command.UserId);
        }
    }
}