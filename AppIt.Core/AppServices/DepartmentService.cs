using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using System.Threading.Tasks;

namespace AppIt.Core.AppServices
{
    public class DepartmentService : IDepartmentService
    {
        public Task<ServiceResponse<CreateDepartmentDto>> CreateDepartmentAsync(CreateDepartmentDto createDto)
        {
            return Task.FromResult(new ServiceResponse<CreateDepartmentDto>(createDto, "Department created (stub)"));
        }

        public Task<ServiceResponse<bool>> DeleteDepartmentAsync(int id)
        {
            return Task.FromResult(new ServiceResponse<bool>(true, "Department deleted (stub)"));
        }

        public Task<ServiceResponse<GetDepartmentDto>> GetDepartmentsAsync(DepartmentFilterDto filterDto)
        {
            return Task.FromResult(new ServiceResponse<GetDepartmentDto>(new GetDepartmentDto(), "Departments retrieved (stub)"));
        }

        public Task<ServiceResponse<DepartmentDto>> GetDepartmentByIdAsync(int id)
        {
            return Task.FromResult(new ServiceResponse<DepartmentDto>(new DepartmentDto(), "Department retrieved (stub)"));
        }

        public Task<ServiceResponse<UpdateDepartmentDto>> UpdateDepartmentAsync(int id, UpdateDepartmentDto updateDto)
        {
            return Task.FromResult(new ServiceResponse<UpdateDepartmentDto>(updateDto, "Department updated (stub)"));
        }
    }
}
