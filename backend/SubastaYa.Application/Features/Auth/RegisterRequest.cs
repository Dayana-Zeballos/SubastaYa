using System.ComponentModel.DataAnnotations;

namespace SubastaYa.Application.Features.Auth;

public class RegisterRequest
{
    [Required]
    [StringLength(64, MinimumLength = 3)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
}
