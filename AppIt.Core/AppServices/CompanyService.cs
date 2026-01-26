using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Core.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly AppItDbContext _context;

        public CompanyService(AppItDbContext context)
        {
            _context = context;
        }

        public async Task<CompanyReadDto> CreateAsync(CreateCompanyDto dto)
        {
            var company = new Company
            {
                CompanyName = dto.CompanyName,
                CompanyAddress = dto.CompanyAddress,
                CompanyEmail = dto.CompanyEmail,
                CompanyPhone = dto.CompanyPhone,
                RegNumber = dto.RegNumber,
                AccountNumber = dto.AccountNumber,
                VatNumber = dto.VatNumber,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            return ToReadDto(company);
        }

        public async Task<CompanyReadDto?> UpdateAsync(UpdateCompanyDto dto)
        {
            var company = await _context.Companies.FindAsync(dto.CompanyId);
            if (company == null) return null;

            company.CompanyName = dto.CompanyName;
            company.CompanyAddress = dto.CompanyAddress;
            company.CompanyEmail = dto.CompanyEmail;
            company.CompanyPhone = dto.CompanyPhone;
            company.RegNumber = dto.RegNumber;
            company.AccountNumber = dto.AccountNumber;
            company.VatNumber = dto.VatNumber;
            company.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return ToReadDto(company);
        }

        public async Task<bool> DeleteAsync(int companyId)
        {
            var company = await _context.Companies.FindAsync(companyId);
            if (company == null) return false;

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CompanyReadDto?> GetByIdAsync(int companyId)
        {
            var company = await _context.Companies.FindAsync(companyId);
            return company == null ? null : ToReadDto(company);
        }

        public async Task<IEnumerable<CompanyReadDto>> GetAllAsync()
        {
            var companies = await _context.Companies.ToListAsync();
            return companies.Select(ToReadDto);
        }

        private CompanyReadDto ToReadDto(Company company) => new()
        {
            CompanyId = company.CompanyId,
            CompanyName = company.CompanyName,
            CompanyAddress = company.CompanyAddress,
            CompanyEmail = company.CompanyEmail,
            CompanyPhone = company.CompanyPhone,
            RegNumber = company.RegNumber,
            AccountNumber = company.AccountNumber,
            VatNumber = company.VatNumber,
            CreatedDate = company.CreatedDate,
            UpdatedDate = company.UpdatedDate
        };
    }
}
