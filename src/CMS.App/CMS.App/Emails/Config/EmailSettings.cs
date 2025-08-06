using System.ComponentModel.DataAnnotations;

namespace CMS.App.Emails.Config;

public class EmailSettings
{
    [Required]
    public SmtpSettings Smtp { get; init; } = default!;

    [Required]
    public string FromName { get; init; } = default!;
}