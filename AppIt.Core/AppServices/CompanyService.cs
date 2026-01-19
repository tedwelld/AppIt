using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AppIt.Core.AppServices
{
    public class CompanyService : ICompanyService
    {
        private readonly AppItDbContext _context;

        public CompanyService(AppItDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse<bool>> CompanyEnabledAsync(int companyId, int featureId)
        {
            var exists = await _context.Companies.AnyAsync(c => c.CompanyId == companyId);
            return new ServiceResponse<bool>(exists, exists ? "Company enabled" : "Company not found");
        }

        public async Task<ServiceResponse<CreateCompanyDto>> CreatCompanyAsync(int companyId, int featureId, int permissionId)
        {
            // Parameters aren't used as CreateCompanyDto contains creation data; create basic company for now
            var newCompany = new Company
            {
                CompanyName = "New Company",
                CompanyAddress = string.Empty,
                CompanyEmail = string.Empty,
                CompanyPhone = string.Empty,
                RegNumber = string.Empty,
                AccountNumber = string.Empty,
                VatNumber = string.Empty
            };
            _context.Companies.Add(newCompany);
            await _context.SaveChangesAsync();

            var dto = new CreateCompanyDto();
            return new ServiceResponse<CreateCompanyDto>(dto, "Company created");
        }

        public async Task<ServiceResponse<CompanyDetailsDto>> CompanyDetailAsync()
        {
            var company = await _context.Companies.OrderBy(c => c.CompanyId).FirstOrDefaultAsync();
            if (company == null)
                return new ServiceResponse<CompanyDetailsDto>(new CompanyDetailsDto(), "No company found");

            var dto = new CompanyDetailsDto
            {
                CompanyId = company.CompanyId,
                CompanyName = company.CompanyName,
                CompanyAddress = company.CompanyAddress,
                CompanyEmail = company.CompanyEmail,
                CompanyPhone = company.CompanyPhone,
                CreatedDate = company.CreatedDate,
                UpdatedDate = company.UpdatedDate,
                RegNumber = company.RegNumber,
                AccountNumber = company.AccountNumber,
                VatNumber = company.VatNumber
            };
            return new ServiceResponse<CompanyDetailsDto>(dto, "Company details retrieved");
        }

        public async Task<ServiceResponse<List<CompanyListDto>>> CompanyListAsync(int companyId)
        {
            var query = _context.Companies.AsQueryable();
            if (companyId > 0)
                query = query.Where(c => c.CompanyId == companyId);

            var list = await query.Select(c => new CompanyListDto
            {
                CompanyId = c.CompanyId,
                CompanyName = c.CompanyName,
                CompanyEmail = c.CompanyEmail,
                CompanyPhone = c.CompanyPhone
            }).ToListAsync();

            return new ServiceResponse<List<CompanyListDto>>(list, "Company list retrieved");
        }

        public async Task<ServiceResponse<List<CompanyDropdownDto>>> CompanyDropdownAsync(int companyId)
        {
            var list = await _context.Companies.Select(c => new CompanyDropdownDto
            {
                CompanyId = c.CompanyId,
                CompanyName = c.CompanyName
            }).ToListAsync();

            return new ServiceResponse<List<CompanyDropdownDto>>(list, "Company dropdown retrieved");
        }

        public async Task<ServiceResponse<bool>> DeleteCompanyAsync(int compayId, int featureId)
        {
            var company = await _context.Companies.FindAsync(compayId);
            if (company == null) return new ServiceResponse<bool>(false, "Company not found");
            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            return new ServiceResponse<bool>(true, "Company deleted");
        }

        public async Task<ServiceResponse<List<CompanySummaryDto>>> CompanySummary(int companyId)
        {
            var list = await _context.Companies.Select(c => new CompanySummaryDto
            {
                CompanyId = c.CompanyId,
                CompanyName = c.CompanyName,
                CompanyEmail = c.CompanyEmail,
                CompanyPhone = c.CompanyPhone
            }).ToListAsync();

            return new ServiceResponse<List<CompanySummaryDto>>(list, "Company summary retrieved");
        }

        public async Task<ServiceResponse<UpdateCompanyDto>> UpdateCompanyAsync(int companyId, int featureId, int permissionId)
        {
            var company = await _context.Companies.FindAsync(companyId);
            if (company == null) return new ServiceResponse<UpdateCompanyDto>(new UpdateCompanyDto(), "Company not found");

            // For now, perform a simple update of UpdatedDate
            company.UpdatedDate = System.DateTime.Now;
            _context.Companies.Update(company);
            await _context.SaveChangesAsync();

            return new ServiceResponse<UpdateCompanyDto>(new UpdateCompanyDto(), "Company updated");
        }
    }
}
