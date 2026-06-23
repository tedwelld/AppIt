using AppIt.Core.AppServices;
using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Core.Services
{
    public class TourService : ITourService
    {
        private readonly AppItDbContext _context;

        public TourService(AppItDbContext context)
        {
            _context = context;
        }

        public async Task<TourReadDto> CreateAsync(CreateTourDto dto)
        {
            await EnsureUniqueNameAsync(dto.Name, null);
            await CatalogConstraints.ValidateCategoryAsync(_context, dto.ProductCategoryId);
            var tour = new Tour
            {
                Name = dto.Name.Trim(),
                Description = dto.Description,
                ProductCategoryId = dto.ProductCategoryId,
                MaxPax = dto.MaxPax,
                IsActive = dto.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            _context.Tours.Add(tour);
            await _context.SaveChangesAsync();
            return await ToReadDtoAsync(tour);
        }

        public async Task<TourReadDto?> UpdateAsync(UpdateTourDto dto)
        {
            var tour = await _context.Tours.FindAsync(dto.Id);
            if (tour == null) return null;

            await EnsureUniqueNameAsync(dto.Name, dto.Id);
            await CatalogConstraints.ValidateCategoryAsync(_context, dto.ProductCategoryId);
            tour.Name = dto.Name.Trim();
            tour.Description = dto.Description;
            tour.ProductCategoryId = dto.ProductCategoryId;
            tour.MaxPax = dto.MaxPax;
            tour.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return await ToReadDtoAsync(tour);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var tour = await _context.Tours.FindAsync(id);
            if (tour == null) return false;

            _context.Tours.Remove(tour);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<TourReadDto?> GetByIdAsync(int id)
        {
            var tour = await _context.Tours.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
            return tour == null ? null : await ToReadDtoAsync(tour);
        }

        public async Task<IEnumerable<TourReadDto>> GetAllAsync()
        {
            var tours = await _context.Tours.AsNoTracking().ToListAsync();
            var prices = await PricesForAsync("Tour", tours.Select(t => t.Id));
            var categoryNames = await CategoryNamesForAsync(tours.Select(t => t.ProductCategoryId));
            return tours.Select(t => ToReadDto(t, prices, categoryNames));
        }

        private async Task<TourReadDto> ToReadDtoAsync(Tour tour)
        {
            var prices = await PricesForAsync("Tour", new[] { tour.Id });
            var categoryNames = await CategoryNamesForAsync(new[] { tour.ProductCategoryId });
            return ToReadDto(tour, prices, categoryNames);
        }

        private static TourReadDto ToReadDto(Tour tour, ILookup<int, ServicePriceReadDto> prices, IReadOnlyDictionary<int, string> categoryNames)
        {
            return new TourReadDto
            {
                Id = tour.Id,
                Name = tour.Name,
                Description = tour.Description,
                ProductCategoryId = tour.ProductCategoryId,
                CategoryName = tour.ProductCategoryId.HasValue && categoryNames.TryGetValue(tour.ProductCategoryId.Value, out var name)
                    ? name
                    : null,
                MaxPax = tour.MaxPax,
                IsActive = tour.IsActive,
                CreatedDate = tour.CreatedDate,
                Prices = prices[tour.Id].ToList()
            };
        }

        private async Task<ILookup<int, ServicePriceReadDto>> PricesForAsync(string serviceType, IEnumerable<int> serviceIds)
        {
            var ids = serviceIds.ToList();
            var prices = await _context.ServicePrices
                .AsNoTracking()
                .Where(p => p.ServiceType == serviceType && ids.Contains(p.ServiceId) && p.IsActive)
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

        private async Task<IReadOnlyDictionary<int, string>> CategoryNamesForAsync(IEnumerable<int?> categoryIds)
        {
            var ids = categoryIds.Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();
            if (ids.Count == 0) return new Dictionary<int, string>();

            return await _context.ProductCategories
                .AsNoTracking()
                .Where(c => ids.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name);
        }

        private async Task EnsureUniqueNameAsync(string name, int? currentId)
        {
            var normalized = (name ?? string.Empty).Trim().ToLower();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                throw new InvalidOperationException("Tour name is required.");
            }

            var exists = await _context.Tours.AnyAsync(t =>
                t.Id != currentId
                && (t.Name ?? string.Empty).ToLower() == normalized);
            if (exists)
            {
                throw new InvalidOperationException($"A tour named '{(name ?? string.Empty).Trim()}' already exists.");
            }
        }
    }
}
