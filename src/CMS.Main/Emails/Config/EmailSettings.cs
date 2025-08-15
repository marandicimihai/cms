using System.ComponentModel.DataAnnotations;

namespace CMS.Main.Emails.Config;

public class EmailSettings
{
    [Required]
    public SmtpSettings Smtp { get; init; } = default!;

    [Required]
    public string FromName { get; init; } = default!;
}