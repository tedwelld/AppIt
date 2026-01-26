using AppIt.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppIt.Core.Interfaces.Services
{
    public interface IDepartmentService
    {
        Task<DepartmentReadDto> CreateAsync(CreateDepartmentDto dto);
        Task<DepartmentReadDto?> UpdateAsync(UpdateDepartmentDto dto);
        Task<bool> DeleteAsync(int id);
        Task<DepartmentReadDto?> GetByIdAsync(int id);
        Task<IEnumerable<DepartmentReadDto>> GetAllAsync();
    }
}
