using AppIt.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppIt.Core.Interfaces.Services
{
    public interface IReservationService
    {
        Task<ReservationReadDto> CreateAsync(CreateReservationDto dto);
        Task<ReservationReadDto?> UpdateAsync(UpdateReservationDto dto);
        Task<bool> DeleteAsync(int id);
        Task<ReservationReadDto?> GetByIdAsync(int id);
        Task<IEnumerable<ReservationReadDto>> GetAllAsync();
    }
}
