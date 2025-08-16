using System.ComponentModel.DataAnnotations;

namespace CMS.Main.Emails.Config;

public class SmtpSettings
{
    [Required]
    public string Host { get; set; } = default!;

    public int Port { get; set; }

    [Required]
    [EmailAddress]
    public string Username { get; set; } = default!;

    [Required]
    public string Password { get; set; } = default!;
}