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
            var accommodation = new Accommodation
            {
                Type = dto.Type,
                Description = dto.Description,
                Capacity = dto.Capacity,
                BasePriceUsd = dto.BasePriceUsd,
                IsActive = dto.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            _context.Accommodations.Add(accommodation);
            await _context.SaveChangesAsync();

            return ToReadDto(accommodation);
        }

        public async Task<AccommodationReadDto?> UpdateAsync(UpdateAccommodationDto dto)
        {
            var accommodation = await _context.Accommodations.FindAsync(dto.Id);
            if (accommodation == null) return null;

            accommodation.Type = dto.Type;
            accommodation.Description = dto.Description;
            accommodation.Capacity = dto.Capacity;
            accommodation.BasePriceUsd = dto.BasePriceUsd;
            accommodation.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return ToReadDto(accommodation);
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
            return accommodation == null ? null : ToReadDto(accommodation);
        }

        public async Task<IEnumerable<AccommodationReadDto>> GetAllAsync()
        {
            return await _context.Accommodations.AsNoTracking()
                .Select(a => new AccommodationReadDto
                {
                    Id = a.Id,
                    Type = a.Type,
                    Description = a.Description,
                    Capacity = a.Capacity,
                    BasePriceUsd = a.BasePriceUsd,
                    IsActive = a.IsActive,
                    CreatedDate = a.CreatedDate
                })
                .ToListAsync();
        }

        public async Task<AccommodationAvailabilityDto> GetAvailabilityAsync(int year, int month)
        {
            var accommodationTypes = await _context.Accommodations
                .AsNoTracking()
                .Where(a => a.IsActive)
                .GroupBy(a => a.Type)
                .Select(g => new { Type = g.Key, TotalCapacity = g.Sum(a => a.Capacity) })
                .ToListAsync();

            var typeNames = accommodationTypes.Select(t => t.Type).ToList();
            var typeCapacityMap = accommodationTypes.ToDictionary(t => t.Type, t => t.TotalCapacity);

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

        private static AccommodationReadDto ToReadDto(Accommodation accommodation)
        {
            return new AccommodationReadDto
            {
                Id = accommodation.Id,
                Type = accommodation.Type,
                Description = accommodation.Description,
                Capacity = accommodation.Capacity,
                BasePriceUsd = accommodation.BasePriceUsd,
                IsActive = accommodation.IsActive,
                CreatedDate = accommodation.CreatedDate
            };
        }
    }
}
