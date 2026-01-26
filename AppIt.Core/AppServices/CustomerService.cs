using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Core.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly AppItDbContext _context;

        public CustomerService(AppItDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerReadDto> CreateAsync(CreateCustomerDto dto)
        {
            var customer = new Customer
            {
                Title = dto.Title,
                FirstName = dto.FirstName,
                Surname = dto.Surname,
                IdType = dto.IdType,
                Nationality = dto.Nationality,
                Dob = dto.Dob,
                Profession = dto.Profession,
                ProxyName = dto.ProxyName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Image = dto.Image,
                TaxCategory = dto.TaxCategory,
                Region = dto.Region,
                DurationOfStayDays = dto.DurationOfStayDays,
                LastSavedBy = dto.LastSavedBy,
                DateUpdated = DateTime.Now,
                Notes = dto.Notes,
                CustomerType = dto.CustomerTypeId.HasValue
                        ? await _context.Set<CustomerType>().FindAsync(dto.CustomerTypeId.Value)
                    : null
            };

            _context.Set<Customer>().Add(customer);
            await _context.SaveChangesAsync();

            return ToReadDto(customer);
        }

        public async Task<CustomerReadDto?> UpdateAsync(UpdateCustomerDto dto)
        {
            var customer = await _context.Set<Customer>()
                .Include(c => c.Reservations)
                .Include(c => c.CustomerType)
                .FirstOrDefaultAsync(c => c.Id == dto.Id);

            if (customer == null) return null;

            customer.Title = dto.Title;
            customer.FirstName = dto.FirstName;
            customer.Surname = dto.Surname;
            customer.IdType = dto.IdType;
            customer.Nationality = dto.Nationality;
            customer.Dob = dto.Dob;
            customer.Profession = dto.Profession;
            customer.ProxyName = dto.ProxyName;
            customer.Email = dto.Email;
            customer.PhoneNumber = dto.PhoneNumber;
            customer.Image = dto.Image;
            customer.TaxCategory = dto.TaxCategory;
            customer.Region = dto.Region;
            customer.DurationOfStayDays = dto.DurationOfStayDays;
           
            customer.LastSavedBy = dto.LastSavedBy;
            customer.DateUpdated = DateTime.Now;
            customer.Notes = dto.Notes;

            customer.CustomerType = dto.CustomerTypeId.HasValue
                ? await _context.Set<CustomerType>().FindAsync(dto.CustomerTypeId.Value)
                : null;

            await _context.SaveChangesAsync();
            return ToReadDto(customer);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var customer = await _context.Set<Customer>().FindAsync(id);
            if (customer == null) return false;

            _context.Set<Customer>().Remove(customer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CustomerReadDto?> GetByIdAsync(int id)
        {
            var customer = await _context.Set<Customer>()
                .Include(c => c.Reservations)
                .Include(c => c.CustomerType)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            return customer == null ? null : ToReadDto(customer);
        }

        public async Task<IEnumerable<CustomerReadDto>> GetAllAsync()
        {
            var customers = await _context.Set<Customer>()
                .Include(c => c.Reservations)
                .Include(c => c.CustomerType)
                .AsNoTracking()
                .ToListAsync();

            return customers.Select(ToReadDto);
        }

        private CustomerReadDto ToReadDto(Customer c) => new()
        {
            Id = c.Id,
            Title = c.Title,
            FirstName = c.FirstName,
            Surname = c.Surname,
            IdType = c.IdType,
            Nationality = c.Nationality,
            Dob = c.Dob,
            Profession = c.Profession,
            ProxyName = c.ProxyName,
            Email = c.Email,
            PhoneNumber = c.PhoneNumber,
            Image = c.Image,
            TaxCategory = c.TaxCategory,
            Region = c.Region,
            DurationOfStayDays = c.DurationOfStayDays,
            
            LastSavedBy = c.LastSavedBy,
            DateUpdated = c.DateUpdated,
            Notes = c.Notes,
            CustomerTypeId = c.CustomerType?.Id,
            ReservationIds = c.Reservations?.Select(r => r.ReservationId)
        };
    }
}
