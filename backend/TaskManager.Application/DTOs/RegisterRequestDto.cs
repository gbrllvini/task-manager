using System.ComponentModel.DataAnnotations;

namespace TaskManager.Application.DTOs;

public class RegisterRequestDto {
    [Required]
    [StringLength(120, MinimumLength = 3)]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}
