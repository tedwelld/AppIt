using AppIt.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppIt.Core.Interfaces.Services
{
    public interface ICustomerTypeService
    {
        Task<CustomerTypeReadDto> CreateAsync(CreateCustomerTypeDto dto);
        Task<CustomerTypeReadDto?> UpdateAsync(UpdateCustomerTypeDto dto);
        Task<bool> DeleteAsync(int id);
        Task<CustomerTypeReadDto?> GetByIdAsync(int id);
        Task<IEnumerable<CustomerTypeReadDto>> GetAllAsync();
    }
}
