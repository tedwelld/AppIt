using AppIt.Core.DTOs;
using AppIt.Data.Entities;
using AppIt.Data.Entities.AppIt.Core.DTOs;

namespace AppIt.Core.Interfaces.Services
{
    public interface IReportSnapshotService
    {
        Task<IEnumerable<ReportSnapshotDto>> GetByReportKeyAsync(string reportKey);
        Task<ReportSnapshotDetailDto?> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateReportSnapshotDto dto);
    }
}
