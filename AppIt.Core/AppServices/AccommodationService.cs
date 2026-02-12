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
