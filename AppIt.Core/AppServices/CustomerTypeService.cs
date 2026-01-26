using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Core.Services
{
    public class CustomerTypeService : ICustomerTypeService
    {
        private readonly AppItDbContext _context;

        public CustomerTypeService(AppItDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerTypeReadDto> CreateAsync(CreateCustomerTypeDto dto)
        {
            var customer = await _context.Set<Customer>().FindAsync(dto.CustomerId);
            if (customer == null) throw new ArgumentException("Customer not found");

            var customerType = new CustomerType
            {
                CustomerId = dto.CustomerId,
                Customer = customer,
                Family = dto.Family,
                CustomerGroup = dto.CustomerGroup,
                GroupNumber = dto.GroupNumber,
                TaxationPercentage = dto.TaxationPercentage,
                Age = dto.Age,
                SpecialPice = dto.SpecialPrice,
                Disability = dto.Disability,
                LastSavedBy = dto.LastSavedBy,
                DateUpdated = DateTime.Now,
                Notes = dto.Notes
            };

            _context.Set<CustomerType>().Add(customerType);
            await _context.SaveChangesAsync();

            return ToReadDto(customerType);
        }

        public async Task<CustomerTypeReadDto?> UpdateAsync(UpdateCustomerTypeDto dto)
        {
            var customerType = await _context.Set<CustomerType>()
                .Include(ct => ct.Reservations)
                .FirstOrDefaultAsync(ct => ct.Id == dto.Id);

            if (customerType == null) return null;

            customerType.Family = dto.Family;
            customerType.CustomerGroup = dto.CustomerGroup;
            customerType.GroupNumber = dto.GroupNumber;
            customerType.TaxationPercentage = dto.TaxationPercentage;
            customerType.Age = dto.Age;
            customerType.SpecialPice = dto.SpecialPrice;
            customerType.Disability = dto.Disability;
            customerType.LastSavedBy = dto.LastSavedBy;
            customerType.DateUpdated = DateTime.Now;
            customerType.Notes = dto.Notes;

            await _context.SaveChangesAsync();
            return ToReadDto(customerType);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var customerType = await _context.Set<CustomerType>().FindAsync(id);
            if (customerType == null) return false;

            _context.Set<CustomerType>().Remove(customerType);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CustomerTypeReadDto?> GetByIdAsync(int id)
        {
            var customerType = await _context.Set<CustomerType>()
                .Include(ct => ct.Reservations)
                .AsNoTracking()
                .FirstOrDefaultAsync(ct => ct.Id == id);

            return customerType == null ? null : ToReadDto(customerType);
        }

        public async Task<IEnumerable<CustomerTypeReadDto>> GetAllAsync()
        {
            var customerTypes = await _context.Set<CustomerType>()
                .Include(ct => ct.Reservations)
                .AsNoTracking()
                .ToListAsync();

            return customerTypes.Select(ToReadDto);
        }

        private CustomerTypeReadDto ToReadDto(CustomerType ct) => new()
        {
            Id = ct.Id,
            CustomerId = ct.CustomerId,
            Family = ct.Family,
            CustomerGroup = ct.CustomerGroup,
            GroupNumber = ct.GroupNumber,
            TaxationPercentage = ct.TaxationPercentage,
            Age = ct.Age,
            SpecialPrice = ct.SpecialPice,
            Disability = ct.Disability,
            LastSavedBy = ct.LastSavedBy,
            DateUpdated = ct.DateUpdated,
            Notes = ct.Notes,
            ReservationIds = ct.Reservations?.Select(r => r.ReservationId)
        };
    }
}
