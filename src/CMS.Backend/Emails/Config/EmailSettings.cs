using System.ComponentModel.DataAnnotations;

namespace CMS.Backend.Emails.Models;

public class EmailSettings
{
    [Required]
    public SmtpSettings Smtp { get; init; } = default!;

    [Required]
    public string FromName { get; init; } = default!;
}