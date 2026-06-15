using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AppIt.Api.Infrastructure;
using AppIt.Core.DTOs;
using AppIt.Core.DTOs.Notifications;
using AppIt.Core.Interfaces;
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
        private readonly ICurrentUserService _currentUser;

        public NotificationsController(
            INotificationService notificationService,
            ICurrentUserService currentUser)
        {
            _notificationService = notificationService;
            _currentUser = currentUser;
        }

        [HttpGet]
        public async Task<IActionResult> GetMine([FromQuery] ListQueryOptions query)
        {
            if (!_currentUser.UserId.HasValue)
                return Unauthorized();

            var notifications = await _notificationService.GetByUserAsync(_currentUser.UserId.Value);
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

            if (!_currentUser.IsStaff && notification.UserId != _currentUser.UserId)
            {
                return Forbid();
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

            if (!_currentUser.IsStaff && existing.UserId != _currentUser.UserId)
            {
                return Forbid();
            }

            await _notificationService.MarkAsReadAsync(id);
            return NoContent();
        }
    }
}
