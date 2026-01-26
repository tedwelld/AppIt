using AppIt.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppIt.Core.Interfaces.Services
{
    public interface ICompanyService
    {
        Task<CompanyReadDto> CreateAsync(CreateCompanyDto dto);
        Task<CompanyReadDto?> UpdateAsync(UpdateCompanyDto dto);
        Task<bool> DeleteAsync(int companyId);
        Task<CompanyReadDto?> GetByIdAsync(int companyId);
        Task<IEnumerable<CompanyReadDto>> GetAllAsync();
    }
}
