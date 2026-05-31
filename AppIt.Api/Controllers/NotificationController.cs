using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AppIt.Api.Infrastructure;
using AppIt.Core.DTOs;
using AppIt.Core.DTOs.Notifications;
using AppIt.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMine([FromQuery] ListQueryOptions query)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (!int.TryParse(userIdValue, out var userId))
                return Unauthorized();

            var notifications = await _notificationService.GetByUserAsync(userId);
            return Ok(notifications.ApplyQuery(query,
                nameof(NotificationDto.Title),
                nameof(NotificationDto.Message)));
        }

        [HttpGet("user/{userId:int}")]
        [Authorize(Roles = "super,admin")]
        public async Task<IActionResult> GetByUser(int userId, [FromQuery] ListQueryOptions query)
        {
            var notifications = await _notificationService.GetByUserAsync(userId);
            return Ok(notifications.ApplyQuery(query,
                nameof(NotificationDto.Title),
                nameof(NotificationDto.Message)));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var notification = await _notificationService.GetByIdAsync(id);

            if (notification == null)
            {
                return NotFound();
            }

            return Ok(notification);
        }

        [HttpPost]
        [Authorize(Roles = "super,admin")]
        public async Task<IActionResult> Create(CreateNotificationDto dto)
        {
            var id = await _notificationService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id }, null);
        }

        [HttpPut("{id:int}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var existing = await _notificationService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            await _notificationService.MarkAsReadAsync(id);
            return NoContent();
        }
    }
}
