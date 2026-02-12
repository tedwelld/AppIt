using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Core.Services
{
    public class ActivityService : IActivityService
    {
        private readonly AppItDbContext _context;

        public ActivityService(AppItDbContext context)
        {
            _context = context;
        }

        public async Task<ActivityReadDto> CreateAsync(CreateActivityDto dto)
        {
            var activity = new Activity
            {
                Name = dto.Name,
                Description = dto.Description,
                BasePriceUsd = dto.BasePriceUsd,
                IsActive = dto.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

            return ToReadDto(activity);
        }

        public async Task<ActivityReadDto?> UpdateAsync(UpdateActivityDto dto)
        {
            var activity = await _context.Activities.FindAsync(dto.Id);
            if (activity == null) return null;

            activity.Name = dto.Name;
            activity.Description = dto.Description;
            activity.BasePriceUsd = dto.BasePriceUsd;
            activity.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return ToReadDto(activity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var activity = await _context.Activities.FindAsync(id);
            if (activity == null) return false;

            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ActivityReadDto?> GetByIdAsync(int id)
        {
            var activity = await _context.Activities.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            return activity == null ? null : ToReadDto(activity);
        }

        public async Task<IEnumerable<ActivityReadDto>> GetAllAsync()
        {
            return await _context.Activities.AsNoTracking()
                .Select(a => new ActivityReadDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description,
                    BasePriceUsd = a.BasePriceUsd,
                    IsActive = a.IsActive,
                    CreatedDate = a.CreatedDate
                })
                .ToListAsync();
        }

        private static ActivityReadDto ToReadDto(Activity activity)
        {
            return new ActivityReadDto
            {
                Id = activity.Id,
                Name = activity.Name,
                Description = activity.Description,
                BasePriceUsd = activity.BasePriceUsd,
                IsActive = activity.IsActive,
                CreatedDate = activity.CreatedDate
            };
        }
    }
}
