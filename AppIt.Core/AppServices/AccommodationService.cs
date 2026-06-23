using AppIt.Core.AppServices;
using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Core.Services
{
    public class AccommodationService : IAccommodationService
    {
        private readonly AppItDbContext _context;

        public AccommodationService(AppItDbContext context)
        {
            _context = context;
        }

        public async Task<AccommodationReadDto> CreateAsync(CreateAccommodationDto dto)
        {
            await EnsureUniqueTypeAsync(dto.Type, null);
            await CatalogConstraints.ValidateCategoryAsync(_context, dto.ProductCategoryId);
            var accommodation = new Accommodation
            {
                Type = dto.Type.Trim(),
                Description = dto.Description,
                ProductCategoryId = dto.ProductCategoryId,
                Capacity = dto.Capacity,
                GuestCapacity = dto.GuestCapacity,
                BasePriceUsd = dto.BasePriceUsd,
                IsActive = dto.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            _context.Accommodations.Add(accommodation);
            await _context.SaveChangesAsync();
            await EnsureUsdPriceAsync(accommodation.Id, accommodation.BasePriceUsd);

            return await ToReadDtoAsync(accommodation);
        }

        public async Task<AccommodationReadDto?> UpdateAsync(UpdateAccommodationDto dto)
        {
            var accommodation = await _context.Accommodations.FindAsync(dto.Id);
            if (accommodation == null) return null;

            await EnsureUniqueTypeAsync(dto.Type, dto.Id);
            await CatalogConstraints.ValidateCategoryAsync(_context, dto.ProductCategoryId);
            accommodation.Type = dto.Type.Trim();
            accommodation.Description = dto.Description;
            accommodation.ProductCategoryId = dto.ProductCategoryId;
            accommodation.Capacity = dto.Capacity;
            accommodation.GuestCapacity = dto.GuestCapacity;
            accommodation.BasePriceUsd = dto.BasePriceUsd;
            accommodation.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            await EnsureUsdPriceAsync(accommodation.Id, accommodation.BasePriceUsd);
            return await ToReadDtoAsync(accommodation);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var accommodation = await _context.Accommodations.FindAsync(id);
            if (accommodation == null) return false;

            _context.Accommodations.Remove(accommodation);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<AccommodationReadDto?> GetByIdAsync(int id)
        {
            var accommodation = await _context.Accommodations.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            return accommodation == null ? null : await ToReadDtoAsync(accommodation);
        }

        public async Task<IEnumerable<AccommodationReadDto>> GetAllAsync()
        {
            var accommodations = await _context.Accommodations.AsNoTracking().ToListAsync();
            var prices = await PricesForAsync(accommodations.Select(a => a.Id));
            var categoryNames = await CategoryNamesForAsync(accommodations.Select(a => a.ProductCategoryId));
            return accommodations.Select(a => ToReadDto(a, prices, categoryNames));
        }

        public async Task<AccommodationAvailabilityDto> GetAvailabilityAsync(int year, int month)
        {
            var accommodationTypes = await _context.Accommodations
                .AsNoTracking()
                .Where(a => a.IsActive)
                .GroupBy(a => a.Type)
                .Select(g => new
                {
                    Type = g.Key,
                    TotalCapacity = g.Sum(a => a.Capacity),
                    MinGuestCapacity = g.Min(a => a.GuestCapacity),
                    MaxGuestCapacity = g.Max(a => a.GuestCapacity)
                })
                .ToListAsync();

            var typeNames = accommodationTypes.Select(t => t.Type).ToList();
            var typeCapacityMap = accommodationTypes.ToDictionary(t => t.Type, t => t.TotalCapacity);
            var typeMinGuestCapacityMap = accommodationTypes.ToDictionary(t => t.Type, t => t.MinGuestCapacity);
            var typeMaxGuestCapacityMap = accommodationTypes.ToDictionary(t => t.Type, t => t.MaxGuestCapacity);

            var monthStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1).AddDays(1).AddTicks(-1);

            var accommodationIdToType = await _context.Accommodations
                .AsNoTracking()
                .Where(a => a.IsActive)
                .ToDictionaryAsync(a => a.Id, a => a.Type);

            var rawItems = await _context.ReservationServiceItems
                .AsNoTracking()
                .Include(i => i.Reservation)
                .Where(i => i.ServiceType == "Accommodation"
                    && i.Reservation != null
                    && i.Reservation.CreatedDate >= monthStart
                    && i.Reservation.CreatedDate <= monthEnd)
                .Select(i => new
                {
                    Day = i.Reservation!.CreatedDate.Day,
                    ServiceId = i.ServiceId,
                    Quantity = i.Quantity
                })
                .ToListAsync();

            var bookedByDayType = rawItems
                .GroupBy(i => new { i.Day, Type = accommodationIdToType.GetValueOrDefault(i.ServiceId, "Unknown") })
                .Select(g => new { g.Key.Day, g.Key.Type, Booked = g.Sum(i => i.Quantity) })
                .ToList();

            var daysInMonth = DateTime.DaysInMonth(year, month);
            var firstDayOfMonth = new DateTime(year, month, 1);
            var startPadding = (int)firstDayOfMonth.DayOfWeek;

            var days = new List<DayAvailabilityDto>();

            for (var i = 0; i < startPadding; i++)
            {
                var padDate = firstDayOfMonth.AddDays(-startPadding + i);
                days.Add(new DayAvailabilityDto
                {
                    Day = padDate.Day,
                    DayOfWeek = padDate.DayOfWeek.ToString(),
                    InMonth = false,
                    TypeAvailability = typeNames.Select(t => new TypeAvailabilityDto
                    {
                        Type = t,
                        TotalCapacity = typeCapacityMap[t],
                        MinGuestCapacity = typeMinGuestCapacityMap[t],
                        MaxGuestCapacity = typeMaxGuestCapacityMap[t],
                        Booked = 0,
                        Available = typeCapacityMap[t]
                    }).ToList()
                });
            }

            for (var day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(year, month, day);
                days.Add(new DayAvailabilityDto
                {
                    Day = day,
                    DayOfWeek = date.DayOfWeek.ToString(),
                    InMonth = true,
                    TypeAvailability = typeNames.Select(t =>
                    {
                        var total = typeCapacityMap[t];
                        var booked = bookedByDayType.Where(b => b.Day == day && b.Type == t).Sum(b => b.Booked);
                        return new TypeAvailabilityDto
                        {
                            Type = t,
                            TotalCapacity = total,
                            MinGuestCapacity = typeMinGuestCapacityMap[t],
                            MaxGuestCapacity = typeMaxGuestCapacityMap[t],
                            Booked = booked,
                            Available = total - booked
                        };
                    }).ToList()
                });
            }

            var remaining = 42 - days.Count;
            for (var i = 1; i <= remaining; i++)
            {
                var padDate = new DateTime(year, month, daysInMonth).AddDays(i);
                days.Add(new DayAvailabilityDto
                {
                    Day = padDate.Day,
                    DayOfWeek = padDate.DayOfWeek.ToString(),
                    InMonth = false,
                    TypeAvailability = typeNames.Select(t => new TypeAvailabilityDto
                    {
                        Type = t,
                        TotalCapacity = typeCapacityMap[t],
                        MinGuestCapacity = typeMinGuestCapacityMap[t],
                        MaxGuestCapacity = typeMaxGuestCapacityMap[t],
                        Booked = 0,
                        Available = typeCapacityMap[t]
                    }).ToList()
                });
            }

            return new AccommodationAvailabilityDto
            {
                Year = year,
                Month = month,
                Types = typeNames,
                Days = days
            };
        }

        private async Task EnsureUsdPriceAsync(int accommodationId, decimal basePriceUsd)
        {
            var price = await _context.ServicePrices
                .FirstOrDefaultAsync(p => p.ServiceType == "Accommodation" && p.ServiceId == accommodationId && p.CurrencyCode == "USD" && p.IsActive);
            if (price == null)
            {
                _context.ServicePrices.Add(new ServicePrice
                {
                    ServiceType = "Accommodation",
                    ServiceId = accommodationId,
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

        private async Task<AccommodationReadDto> ToReadDtoAsync(Accommodation accommodation)
        {
            var prices = await PricesForAsync(new[] { accommodation.Id });
            var categoryNames = await CategoryNamesForAsync(new[] { accommodation.ProductCategoryId });
            return ToReadDto(accommodation, prices, categoryNames);
        }

        private static AccommodationReadDto ToReadDto(
            Accommodation accommodation,
            ILookup<int, ServicePriceReadDto> prices,
            IReadOnlyDictionary<int, string> categoryNames)
        {
            return new AccommodationReadDto
            {
                Id = accommodation.Id,
                Type = accommodation.Type,
                Description = accommodation.Description,
                ProductCategoryId = accommodation.ProductCategoryId,
                CategoryName = accommodation.ProductCategoryId.HasValue && categoryNames.TryGetValue(accommodation.ProductCategoryId.Value, out var name)
                    ? name
                    : null,
                Capacity = accommodation.Capacity,
                GuestCapacity = accommodation.GuestCapacity,
                BasePriceUsd = accommodation.BasePriceUsd,
                IsActive = accommodation.IsActive,
                CreatedDate = accommodation.CreatedDate,
                Prices = prices[accommodation.Id].ToList()
            };
        }

        private async Task<ILookup<int, ServicePriceReadDto>> PricesForAsync(IEnumerable<int> accommodationIds)
        {
            var ids = accommodationIds.ToList();
            var prices = await _context.ServicePrices
                .AsNoTracking()
                .Where(p => p.ServiceType == "Accommodation" && ids.Contains(p.ServiceId) && p.IsActive)
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

        private async Task EnsureUniqueTypeAsync(string type, int? currentId)
        {
            var normalized = (type ?? string.Empty).Trim().ToLower();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                throw new InvalidOperationException("Accommodation name is required.");
            }

            var exists = await _context.Accommodations.AnyAsync(a =>
                a.Id != currentId
                && (a.Type ?? string.Empty).ToLower() == normalized);
            if (exists)
            {
                throw new InvalidOperationException($"An accommodation named '{type.Trim()}' already exists.");
            }
        }
    }
}
