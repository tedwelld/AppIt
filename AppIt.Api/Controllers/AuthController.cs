using AppIt.Api.Services;
using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using AppIt.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AppIt.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly JwtTokenService _jwtTokenService;
        private readonly AppItDbContext _db;

        public AuthController(
            IAuthService authService,
            IHostEnvironment hostEnvironment,
            JwtTokenService jwtTokenService,
            AppItDbContext db)
        {
            _authService = authService;
            _hostEnvironment = hostEnvironment;
            _jwtTokenService = jwtTokenService;
            _db = db;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.RegisterAsync(dto, HttpContext.Connection.RemoteIpAddress?.ToString());
            response.Token = _jwtTokenService.GenerateToken(response.User);
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.LoginAsync(dto, HttpContext.Connection.RemoteIpAddress?.ToString());
            response.Token = _jwtTokenService.GenerateToken(response.User);
            return Ok(response);
        }

        [HttpPost("password-reset/request")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.RequestPasswordResetAsync(
                dto,
                includeRawToken: _hostEnvironment.IsDevelopment(),
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(response);
        }

        [HttpPost("password-reset/confirm")]
        public async Task<IActionResult> ConfirmPasswordReset([FromBody] PasswordResetConfirmDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _authService.ResetPasswordAsync(dto, HttpContext.Connection.RemoteIpAddress?.ToString());
            return NoContent();
        }

        [Authorize]
        [HttpGet("permissions")]
        public async Task<IActionResult> GetMyPermissions()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var roleId = await _db.Accounts
                .AsNoTracking()
                .Where(a => a.Id == userId)
                .Select(a => (int?)a.RoleId)
                .FirstOrDefaultAsync();
            if (!roleId.HasValue)
            {
                return Ok(Array.Empty<string>());
            }

            if (User.IsInRole("super"))
            {
                var all = await _db.Permissions.AsNoTracking().Select(p => p.Name).ToListAsync();
                return Ok(all);
            }

            var permissions = await _db.RoleFeaturePermissions
                .AsNoTracking()
                .Where(rfp => rfp.RoleId == roleId.Value && rfp.IsActivated)
                .Join(_db.Permissions.AsNoTracking(),
                    rfp => rfp.PermissionId,
                    p => p.PermissionId,
                    (_, p) => p.Name)
                .Distinct()
                .ToListAsync();

            return Ok(permissions);
        }
    }
}
