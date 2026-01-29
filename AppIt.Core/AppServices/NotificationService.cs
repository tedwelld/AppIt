using AppIt.Core.DTOs;
using AppIt.Core.DTOs.Notifications;
using AppIt.Core.Interfaces;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using AppIt.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace AppIt.Core.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppItDbContext _context;

        public NotificationService(AppItDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<NotificationDto>> GetByUserAsync(int userId)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<NotificationDetailDto?> GetByIdAsync(int id)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(n => n.Id == id)
                .Select(n => new NotificationDetailDto
                {
                    Id = n.Id,
                    UserId = n.UserId,
                    Title = n.Title,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    ReadAt = n.ReadAt,
                    CreatedAt = n.CreatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<int> CreateAsync(CreateNotificationDto dto)
        {
            var notification = new Notification
            {
                UserId = dto.UserId,
                Title = dto.Title,
                Message = dto.Message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return notification.Id;
        }

        public async Task MarkAsReadAsync(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null || notification.IsRead)
                return;

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}
