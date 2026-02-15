using AppIt.Core.DTOs;

namespace AppIt.Core.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto, string? ipAddress = null);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto dto, string? ipAddress = null);
        Task<PasswordResetRequestResponseDto> RequestPasswordResetAsync(PasswordResetRequestDto dto, bool includeRawToken = false, string? ipAddress = null);
        Task ResetPasswordAsync(PasswordResetConfirmDto dto, string? ipAddress = null);
    }
}
