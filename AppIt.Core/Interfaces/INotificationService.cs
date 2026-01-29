using AppIt.Core.DTOs.Notifications;

namespace AppIt.Core.Interfaces.Services
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDto>> GetByUserAsync(int userId);
        Task<NotificationDetailDto?> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateNotificationDto dto);
        Task MarkAsReadAsync(int id);
    }
}
