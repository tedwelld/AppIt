using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Core.AppServices
{
    public class ComboService : IComboService
    {
        private readonly AppItDbContext _context;

        public ComboService(AppItDbContext context) => _context = context;

        private static ComboReadDto Map(Combo c) => new()
        {
            Id = c.Id, Name = c.Name, Code = c.Code, Description = c.Description,
            SupplierId = c.SupplierId, ProductCategoryId = c.ProductCategoryId,
            MaxProducts = c.MaxProducts, IsActive = c.IsActive,
            Products = c.ComboProducts.Select(p => new ComboProductReadDto
            {
                Id = p.Id, ServiceType = p.ServiceType, ServiceId = p.ServiceId,
                ServiceName = p.ServiceName, IsMandatory = p.IsMandatory
            }).ToList()
        };

        public async Task<IEnumerable<ComboReadDto>> GetAllAsync() =>
            await _context.Combos.AsNoTracking().Include(c => c.ComboProducts)
                .Where(c => c.IsActive).Select(c => Map(c)).ToListAsync();

        public async Task<ComboReadDto?> GetByIdAsync(int id)
        {
            var c = await _context.Combos.AsNoTracking().Include(x => x.ComboProducts).FirstOrDefaultAsync(x => x.Id == id);
            return c == null ? null : Map(c);
        }

        public async Task<ComboReadDto> CreateAsync(CreateComboDto dto)
        {
            var entity = new Combo
            {
                Name = dto.Name.Trim(), Code = dto.Code.Trim(), Description = dto.Description,
                SupplierId = dto.SupplierId, ProductCategoryId = dto.ProductCategoryId,
                MaxProducts = dto.MaxProducts
            };
            foreach (var p in dto.Products)
            {
                entity.ComboProducts.Add(new ComboProduct
                {
                    ServiceType = p.ServiceType, ServiceId = p.ServiceId,
                    ServiceName = p.ServiceName, IsMandatory = p.IsMandatory
                });
            }
            _context.Combos.Add(entity);
            await _context.SaveChangesAsync();
            return (await GetByIdAsync(entity.Id))!;
        }

        public async Task<ComboReadDto?> UpdateAsync(int id, UpdateComboDto dto)
        {
            var entity = await _context.Combos.Include(c => c.ComboProducts).FirstOrDefaultAsync(c => c.Id == id);
            if (entity == null) return null;
            entity.Name = dto.Name.Trim();
            entity.Code = dto.Code.Trim();
            entity.Description = dto.Description;
            entity.IsActive = dto.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            _context.ComboProducts.RemoveRange(entity.ComboProducts);
            foreach (var p in dto.Products)
            {
                entity.ComboProducts.Add(new ComboProduct
                {
                    ServiceType = p.ServiceType, ServiceId = p.ServiceId,
                    ServiceName = p.ServiceName, IsMandatory = p.IsMandatory
                });
            }
            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Combos.FindAsync(id);
            if (entity == null) return false;
            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ReservationServiceItemSplitDto>> ExpandComboSplitsAsync(int comboId, decimal headerTotal, int quantity)
        {
            var products = await _context.ComboProducts.AsNoTracking().Where(p => p.ComboId == comboId).ToListAsync();
            if (products.Count == 0) return Array.Empty<ReservationServiceItemSplitDto>();

            var perLine = decimal.Round(headerTotal / products.Count, 2, MidpointRounding.AwayFromZero);
            var remainder = headerTotal - perLine * products.Count;
            var splits = new List<ReservationServiceItemSplitDto>();
            for (var i = 0; i < products.Count; i++)
            {
                var p = products[i];
                var lineTotal = perLine + (i == 0 ? remainder : 0);
                splits.Add(new ReservationServiceItemSplitDto
                {
                    ServiceType = p.ServiceType, ServiceId = p.ServiceId, ServiceName = p.ServiceName,
                    UnitPrice = lineTotal / quantity, TotalPrice = lineTotal, Quantity = quantity, IsMandatory = p.IsMandatory
                });
            }
            return splits;
        }
    }
}
