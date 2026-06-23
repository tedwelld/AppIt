using AppIt.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppIt.Core.Interfaces.Services
{
    public interface IDayEndService
    {
        Task<IEnumerable<DayEndReadDto>> GetAllAsync();
        Task<DayEndReadDto?> GetByIdAsync(int id);
        Task<DayEndReadDto?> GetTodayAsync();
        Task<DayEndReadDto> OpenAsync(OpenDayEndDto dto, string openedBy);
        Task<DayEndReadDto?> CloseAsync(CloseDayEndDto dto, string closedBy);
        Task<JournalRunResultDto> RunJournalTransactionsAsync(DateTime? processingDate = null);
        Task<int> DeleteExistingJournalEntriesAsync(DateTime? processingDate = null);
    }
}
