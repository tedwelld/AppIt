using AppIt.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppIt.Core.Interfaces.Services
{
    public interface ICustomerService
    {
        Task<CustomerReadDto> CreateAsync(CreateCustomerDto dto);
        Task<CustomerReadDto?> UpdateAsync(UpdateCustomerDto dto);
        Task<bool> DeleteAsync(int id);
        Task<CustomerReadDto?> GetByIdAsync(int id);
        Task<IEnumerable<CustomerReadDto>> GetAllAsync();
    }
}
