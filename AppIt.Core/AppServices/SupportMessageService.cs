using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Core.Services
{
    public class SupportMessageService : ISupportMessageService
    {
        private readonly AppItDbContext _context;

        public SupportMessageService(AppItDbContext context)
        {
            _context = context;
        }

        public async Task<SupportMessageReadDto> CreateAsync(CreateSupportMessageDto dto)
        {
            var message = new SupportMessage
            {
                FromEmail = dto.FromEmail,
                ToEmail = dto.ToEmail,
                Subject = string.IsNullOrWhiteSpace(dto.Subject) ? "Support" : dto.Subject,
                Message = dto.Message,
                Status = dto.Status,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.SupportMessages.Add(message);
            await _context.SaveChangesAsync();

            return ToReadDto(message);
        }

        public async Task<SupportMessageReadDto?> UpdateAsync(UpdateSupportMessageDto dto)
        {
            var message = await _context.SupportMessages.FindAsync(dto.Id);
            if (message == null) return null;

            message.FromEmail = dto.FromEmail;
            message.ToEmail = dto.ToEmail;
            message.Subject = string.IsNullOrWhiteSpace(dto.Subject) ? message.Subject : dto.Subject;
            message.Message = dto.Message;
            message.Status = dto.Status;
            message.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return ToReadDto(message);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var message = await _context.SupportMessages.FindAsync(id);
            if (message == null) return false;

            _context.SupportMessages.Remove(message);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<SupportMessageReadDto?> GetByIdAsync(int id)
        {
            var message = await _context.SupportMessages.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            return message == null ? null : ToReadDto(message);
        }

        public async Task<IEnumerable<SupportMessageReadDto>> GetAllAsync()
        {
            return await _context.SupportMessages.AsNoTracking()
                .Select(m => new SupportMessageReadDto
                {
                    Id = m.Id,
                    FromEmail = m.FromEmail,
                    ToEmail = m.ToEmail,
                    Subject = m.Subject,
                    Message = m.Message,
                    Status = m.Status,
                    CreatedAt = m.CreatedDate,
                    UpdatedAt = m.UpdatedDate
                })
                .ToListAsync();
        }

        private static SupportMessageReadDto ToReadDto(SupportMessage message)
        {
            return new SupportMessageReadDto
            {
                Id = message.Id,
                FromEmail = message.FromEmail,
                ToEmail = message.ToEmail,
                Subject = message.Subject,
                Message = message.Message,
                Status = message.Status,
                CreatedAt = message.CreatedDate,
                UpdatedAt = message.UpdatedDate
            };
        }
    }
}
