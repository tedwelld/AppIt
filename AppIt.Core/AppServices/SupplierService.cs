using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppIt.Core.AppServices
{
    public class SupplierService : ISupplierService
    {
        private readonly AppItDbContext _db;

        public SupplierService(AppItDbContext db)
        {
            _db = db;
        }

        // Create a new supplier
        public async Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto dto)
        {
            await EnsureUniqueSupplierAsync(dto.Name, dto.ContactEmail, null);
            var supplier = new Supplier
            {
                Name = dto.Name.Trim(),
                Description = dto.Description,
                ContactEmail = NormalizeOptional(dto.ContactEmail),
                ContactPhone = dto.ContactPhone
            };

            _db.Suppliers.Add(supplier);
            await _db.SaveChangesAsync();

            return new SupplierDto
            {
                SupplierId = supplier.SupplierId,
                Name = supplier.Name,
                Description = supplier.Description,
                ContactEmail = supplier.ContactEmail,
                ContactPhone = supplier.ContactPhone
            };
        }

        // Delete an existing supplier by Id
        public async Task<bool> DeleteSupplierAsync(int id)
        {
            var supplier = await _db.Suppliers.FindAsync(id);
            if (supplier == null) return false;

            _db.Suppliers.Remove(supplier);
            await _db.SaveChangesAsync();
            return true;
        }

        // Get all suppliers
        public async Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync()
        {
            return await _db.Suppliers
                .AsNoTracking()
                .Select(s => new SupplierDto
                {
                    SupplierId = s.SupplierId,
                    Name = s.Name,
                    Description = s.Description,
                    ContactEmail = s.ContactEmail,
                    ContactPhone = s.ContactPhone
                })
                .ToListAsync();
        }

        // Get a single supplier by Id
        public async Task<SupplierDto?> GetSupplierByIdAsync(int id)
        {
            var supplier = await _db.Suppliers.FindAsync(id);
            if (supplier == null) return null;

            return new SupplierDto
            {
                SupplierId = supplier.SupplierId,
                Name = supplier.Name,
                Description = supplier.Description,
                ContactEmail = supplier.ContactEmail,
                ContactPhone = supplier.ContactPhone
            };
        }

        // Update an existing supplier
        public async Task<SupplierDto?> UpdateSupplierAsync(int id, UpdateSupplierDto dto)
        {
            var supplier = await _db.Suppliers.FindAsync(id);
            if (supplier == null) return null;

            await EnsureUniqueSupplierAsync(dto.Name, dto.ContactEmail, id);
            supplier.Name = dto.Name.Trim();
            supplier.Description = dto.Description;
            supplier.ContactEmail = NormalizeOptional(dto.ContactEmail);
            supplier.ContactPhone = dto.ContactPhone;

            await _db.SaveChangesAsync();

            return new SupplierDto
            {
                SupplierId = supplier.SupplierId,
                Name = supplier.Name,
                Description = supplier.Description,
                ContactEmail = supplier.ContactEmail,
                ContactPhone = supplier.ContactPhone
            };
        }

        private async Task EnsureUniqueSupplierAsync(string name, string? email, int? currentId)
        {
            var normalizedName = (name ?? string.Empty).Trim().ToLower();
            var normalizedEmail = NormalizeOptional(email)?.ToLower();
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                throw new InvalidOperationException("Supplier name is required.");
            }

            var exists = await _db.Suppliers.AnyAsync(s =>
                s.SupplierId != currentId
                && (
                    s.Name.ToLower() == normalizedName
                    || (normalizedEmail != null && s.ContactEmail != null && s.ContactEmail.ToLower() == normalizedEmail)
                ));
            if (exists)
            {
                throw new InvalidOperationException("This supplier already exists.");
            }
        }

        private static string? NormalizeOptional(string? value)
        {
            var normalized = value?.Trim();
            return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
        }
    }
}
