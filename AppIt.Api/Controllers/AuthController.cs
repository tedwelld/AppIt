using AppIt.Api.Services;
using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace AppIt.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly JwtTokenService _jwtTokenService;

        public AuthController(IAuthService authService, IHostEnvironment hostEnvironment, JwtTokenService jwtTokenService)
        {
            _authService = authService;
            _hostEnvironment = hostEnvironment;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            var response = await _authService.RegisterAsync(dto, HttpContext.Connection.RemoteIpAddress?.ToString());
            response.Token = _jwtTokenService.GenerateToken(response.User);
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var response = await _authService.LoginAsync(dto, HttpContext.Connection.RemoteIpAddress?.ToString());
            response.Token = _jwtTokenService.GenerateToken(response.User);
            return Ok(response);
        }

        [HttpPost("password-reset/request")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequestDto dto)
        {
            var response = await _authService.RequestPasswordResetAsync(
                dto,
                includeRawToken: _hostEnvironment.IsDevelopment(),
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(response);
        }

        [HttpPost("password-reset/confirm")]
        public async Task<IActionResult> ConfirmPasswordReset([FromBody] PasswordResetConfirmDto dto)
        {
            await _authService.ResetPasswordAsync(dto, HttpContext.Connection.RemoteIpAddress?.ToString());
            return NoContent();
        }
    }
}
