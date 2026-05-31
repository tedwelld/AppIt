using AppIt.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppIt.Core.Interfaces.Services
{
    public interface IReservationService
    {
        Task<ReservationReadDto> CreateAsync(CreateReservationDto dto);
        Task<ReservationReadDto?> UpdateAsync(UpdateReservationDto dto);
        Task<ReservationDeleteResultDto> DeleteAsync(int id);
        Task<ReservationReadDto?> GetByIdAsync(int id);
        Task<IEnumerable<ReservationReadDto>> GetAllAsync();
        Task<IEnumerable<ReservationReadDto>> GetByAccountIdAsync(int accountId);
        Task<ReservationReadDto?> CloseBookingAsync(int id, string closedBy);
        Task<ReservationReadDto?> CancelBookingAsync(int id, string? reason = null);
        Task<ReservationReadDto?> OpenBookingAsync(int id);
        Task<ReservationReadDto?> CloneBookingAsync(int id, string clonedBy);
        Task<IEnumerable<ReservationSnapshotDto>> GetSnapshotsAsync(int id);
    }
}
