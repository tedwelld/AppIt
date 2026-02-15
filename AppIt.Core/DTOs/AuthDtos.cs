using System.ComponentModel.DataAnnotations;

namespace AppIt.Core.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequestDto
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }
        public string? AvatarUrl { get; set; }
        public string PreferredCurrency { get; set; } = "USD";

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public AccountDto User { get; set; } = new();
    }

    public class PasswordResetRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class PasswordResetRequestResponseDto
    {
        public string Message { get; set; } = "If the account exists, reset instructions have been issued.";
        public string? ResetToken { get; set; }
    }

    public class PasswordResetConfirmDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
