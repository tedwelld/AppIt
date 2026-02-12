using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Core.Services
{
    public class VoucherService : IVoucherService
    {
        private readonly AppItDbContext _context;

        public VoucherService(AppItDbContext context)
        {
            _context = context;
        }

        public async Task<VoucherReadDto> CreateAsync(CreateVoucherDto dto)
        {
            var voucher = new Voucher
            {
                Code = dto.Code,
                Reference = dto.Reference,
                Type = dto.Type,
                ComboReference = dto.ComboReference,
                ReservationId = dto.ReservationId,
                CreatedDate = DateTime.UtcNow
            };

            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();

            return ToReadDto(voucher);
        }

        public async Task<VoucherReadDto?> UpdateAsync(UpdateVoucherDto dto)
        {
            var voucher = await _context.Vouchers.FindAsync(dto.Id);
            if (voucher == null) return null;

            voucher.Code = dto.Code;
            voucher.Reference = dto.Reference;
            voucher.Type = dto.Type;
            voucher.ComboReference = dto.ComboReference;
            voucher.ReservationId = dto.ReservationId;

            await _context.SaveChangesAsync();
            return ToReadDto(voucher);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null) return false;

            _context.Vouchers.Remove(voucher);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<VoucherReadDto?> GetByIdAsync(int id)
        {
            var voucher = await _context.Vouchers.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
            return voucher == null ? null : ToReadDto(voucher);
        }

        public async Task<IEnumerable<VoucherReadDto>> GetAllAsync()
        {
            return await _context.Vouchers.AsNoTracking()
                .Select(v => new VoucherReadDto
                {
                    Id = v.Id,
                    Code = v.Code,
                    Reference = v.Reference,
                    Type = v.Type,
                    ComboReference = v.ComboReference,
                    ReservationId = v.ReservationId,
                    CreatedAt = v.CreatedDate
                })
                .ToListAsync();
        }

        private static VoucherReadDto ToReadDto(Voucher voucher)
        {
            return new VoucherReadDto
            {
                Id = voucher.Id,
                Code = voucher.Code,
                Reference = voucher.Reference,
                Type = voucher.Type,
                ComboReference = voucher.ComboReference,
                ReservationId = voucher.ReservationId,
                CreatedAt = voucher.CreatedDate
            };
        }
    }
}
