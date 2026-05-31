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
            await EnsureUniqueNameAsync(dto.Name, null);
            var activity = new Activity
            {
                Name = dto.Name.Trim(),
                Description = dto.Description,
                BasePriceUsd = dto.BasePriceUsd,
                IsActive = dto.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();
            await EnsureUsdPriceAsync(activity.Id, activity.BasePriceUsd);

            return await ToReadDtoAsync(activity);
        }

        public async Task<ActivityReadDto?> UpdateAsync(UpdateActivityDto dto)
        {
            var activity = await _context.Activities.FindAsync(dto.Id);
            if (activity == null) return null;

            await EnsureUniqueNameAsync(dto.Name, dto.Id);
            activity.Name = dto.Name.Trim();
            activity.Description = dto.Description;
            activity.BasePriceUsd = dto.BasePriceUsd;
            activity.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            await EnsureUsdPriceAsync(activity.Id, activity.BasePriceUsd);
            return await ToReadDtoAsync(activity);
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
            return activity == null ? null : await ToReadDtoAsync(activity);
        }

        public async Task<IEnumerable<ActivityReadDto>> GetAllAsync()
        {
            var activities = await _context.Activities.AsNoTracking().ToListAsync();
            var prices = await PricesForAsync(activities.Select(a => a.Id));
            return activities.Select(a => ToReadDto(a, prices));
        }

        private async Task EnsureUsdPriceAsync(int activityId, decimal basePriceUsd)
        {
            var price = await _context.ServicePrices
                .FirstOrDefaultAsync(p => p.ServiceType == "Activity" && p.ServiceId == activityId && p.CurrencyCode == "USD" && p.IsActive);
            if (price == null)
            {
                _context.ServicePrices.Add(new ServicePrice
                {
                    ServiceType = "Activity",
                    ServiceId = activityId,
                    CurrencyCode = "USD",
                    UnitPrice = basePriceUsd,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                price.UnitPrice = basePriceUsd;
            }

            await _context.SaveChangesAsync();
        }

        private async Task<ActivityReadDto> ToReadDtoAsync(Activity activity)
        {
            var prices = await PricesForAsync(new[] { activity.Id });
            return ToReadDto(activity, prices);
        }

        private static ActivityReadDto ToReadDto(Activity activity, ILookup<int, ServicePriceReadDto> prices)
        {
            return new ActivityReadDto
            {
                Id = activity.Id,
                Name = activity.Name,
                Description = activity.Description,
                BasePriceUsd = activity.BasePriceUsd,
                IsActive = activity.IsActive,
                CreatedDate = activity.CreatedDate,
                Prices = prices[activity.Id].ToList()
            };
        }

        private async Task<ILookup<int, ServicePriceReadDto>> PricesForAsync(IEnumerable<int> activityIds)
        {
            var ids = activityIds.ToList();
            var prices = await _context.ServicePrices
                .AsNoTracking()
                .Where(p => p.ServiceType == "Activity" && ids.Contains(p.ServiceId) && p.IsActive)
                .OrderBy(p => p.CurrencyCode)
                .Select(p => new ServicePriceReadDto
                {
                    Id = p.Id,
                    ServiceType = p.ServiceType,
                    ServiceId = p.ServiceId,
                    CurrencyCode = p.CurrencyCode,
                    UnitPrice = p.UnitPrice,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();
            return prices.ToLookup(p => p.ServiceId);
        }

        private async Task EnsureUniqueNameAsync(string name, int? currentId)
        {
            var normalized = (name ?? string.Empty).Trim().ToLower();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                throw new InvalidOperationException("Activity name is required.");
            }

            var exists = await _context.Activities.AnyAsync(a =>
                a.Id != currentId
                && (a.Name ?? string.Empty).ToLower() == normalized);
            if (exists)
            {
                throw new InvalidOperationException($"An activity named '{(name ?? string.Empty).Trim()}' already exists.");
            }
        }
    }
}
