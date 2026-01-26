using AppIt.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppIt.Core.Interfaces
{
    public interface IAccountService
    {
        Task<ServiceResponse<AccountDto>> CreateAsync(CreateAccountDto dto);
        Task<ServiceResponse<List<AccountDto>>> GetAllAsync();
        Task<ServiceResponse<AccountDto>> GetByIdAsync(int id);
        Task<ServiceResponse<AccountDto>> UpdateAsync(int id, UpdateAccountDto dto);
        Task<ServiceResponse<bool>> DeleteAsync(int id);
    }
}
