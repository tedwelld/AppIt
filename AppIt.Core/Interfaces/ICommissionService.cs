using AppIt.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppIt.Core.Interfaces.Services
{
    public interface ICommissionService
    {
        Task<IEnumerable<CommissionReadDto>> GetAllAsync();
        Task<CommissionReadDto?> GetByIdAsync(int id);
        Task<IEnumerable<CommissionReadDto>> GetByReservationAsync(int reservationId);
        Task<CommissionReadDto> CreateAsync(CreateCommissionDto dto);
        Task<CommissionReadDto?> UpdateAsync(UpdateCommissionDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
