using System.Net.Mail;
using CMS.Main.Emails.Config;

namespace CMS.Main.Emails;

public static class FluentEmailConfiguration
{
    public static IServiceCollection ConfigureFluentEmail(this IServiceCollection services, 
        IConfiguration configuration, 
        IHostEnvironment hostEnvironment)
    {
        var emailSettings = configuration.GetSection("EmailSettings").Get<EmailSettings>();

        var smtp = emailSettings!.Smtp;
        var fluentBuilder = services.AddFluentEmail(smtp.Username, emailSettings.FromName)
            .AddRazorRenderer();

        if (hostEnvironment.IsDevelopment())
        {
            fluentBuilder.AddSmtpSender(new SmtpClient(smtp.Host, smtp.Port)
            {
                EnableSsl = false,
            });
        }
        else
        {
            fluentBuilder.AddSmtpSender(smtp.Host, smtp.Port, smtp.Username, smtp.Password);
        }

        return services;
    }
}