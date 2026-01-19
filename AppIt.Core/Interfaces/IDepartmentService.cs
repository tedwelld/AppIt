using System;
using System.Collections.Generic;
using System.Text;
using AppIt.Core.DTOs;

namespace AppIt.Core.Interfaces
{
    public interface IDepartmentService
    {
        Task<ServiceResponse<GetDepartmentDto>> GetDepartmentsAsync(DepartmentFilterDto filterDto);
        Task<ServiceResponse<DepartmentDto>> GetDepartmentByIdAsync(int id);
        Task<ServiceResponse<CreateDepartmentDto>> CreateDepartmentAsync(CreateDepartmentDto createDto);
        Task<ServiceResponse<UpdateDepartmentDto>> UpdateDepartmentAsync(int id, UpdateDepartmentDto updateDto);
        Task<ServiceResponse<bool>> DeleteDepartmentAsync(int id);


    }
}
