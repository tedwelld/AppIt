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
            var supplier = new Supplier
            {
                Name = dto.Name,
                Description = dto.Description,
                ContactEmail = dto.ContactEmail,
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

            supplier.Name = dto.Name;
            supplier.Description = dto.Description;
            supplier.ContactEmail = dto.ContactEmail;
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
    }
}
