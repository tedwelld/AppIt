using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Core.Services
{
    public class TransferService : ITransferService
    {
        private readonly AppItDbContext _context;

        public TransferService(AppItDbContext context)
        {
            _context = context;
        }

        public async Task<TransferReadDto> CreateAsync(CreateTransferDto dto)
        {
            await EnsureUniqueNameAsync(dto.Name, null);
            var transfer = new Transfer
            {
                Name = dto.Name.Trim(),
                Description = dto.Description,
                IsActive = dto.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            _context.Transfers.Add(transfer);
            await _context.SaveChangesAsync();
            return await ToReadDtoAsync(transfer);
        }

        public async Task<TransferReadDto?> UpdateAsync(UpdateTransferDto dto)
        {
            var transfer = await _context.Transfers.FindAsync(dto.Id);
            if (transfer == null) return null;

            await EnsureUniqueNameAsync(dto.Name, dto.Id);
            transfer.Name = dto.Name.Trim();
            transfer.Description = dto.Description;
            transfer.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return await ToReadDtoAsync(transfer);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var transfer = await _context.Transfers.FindAsync(id);
            if (transfer == null) return false;

            _context.Transfers.Remove(transfer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<TransferReadDto?> GetByIdAsync(int id)
        {
            var transfer = await _context.Transfers.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
            return transfer == null ? null : await ToReadDtoAsync(transfer);
        }

        public async Task<IEnumerable<TransferReadDto>> GetAllAsync()
        {
            var transfers = await _context.Transfers.AsNoTracking().ToListAsync();
            var prices = await PricesForAsync("Transfer", transfers.Select(t => t.Id));
            return transfers.Select(t => ToReadDto(t, prices));
        }

        private async Task<TransferReadDto> ToReadDtoAsync(Transfer transfer)
        {
            var prices = await PricesForAsync("Transfer", new[] { transfer.Id });
            return ToReadDto(transfer, prices);
        }

        private static TransferReadDto ToReadDto(Transfer transfer, ILookup<int, ServicePriceReadDto> prices)
        {
            return new TransferReadDto
            {
                Id = transfer.Id,
                Name = transfer.Name,
                Description = transfer.Description,
                IsActive = transfer.IsActive,
                CreatedDate = transfer.CreatedDate,
                Prices = prices[transfer.Id].ToList()
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

        private async Task EnsureUniqueNameAsync(string name, int? currentId)
        {
            var normalized = (name ?? string.Empty).Trim().ToLower();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                throw new InvalidOperationException("Transfer name is required.");
            }

            var exists = await _context.Transfers.AnyAsync(t =>
                t.Id != currentId
                && (t.Name ?? string.Empty).ToLower() == normalized);
            if (exists)
            {
                throw new InvalidOperationException($"A transfer named '{(name ?? string.Empty).Trim()}' already exists.");
            }
        }
    }
}
