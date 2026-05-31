using AppIt.Core.DTOs;

namespace AppIt.Core.Interfaces
{
    public interface IAuditLogService
    {
        Task<IEnumerable<AuditLogReadDto>> GetAllAsync();
        Task<AuditLogReadDto?> GetByIdAsync(int id);
    }
}
