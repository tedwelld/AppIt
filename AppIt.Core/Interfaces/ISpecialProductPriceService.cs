using AppIt.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppIt.Core.Interfaces.Services
{
    public interface ISpecialProductPriceService
    {
        Task<IEnumerable<SpecialProductPriceReadDto>> GetAllAsync();
        Task<SpecialProductPriceReadDto?> GetByIdAsync(int id);
        Task<IEnumerable<SpecialProductPriceReadDto>> GetByProductAsync(int productId);
        Task<SpecialProductPriceReadDto> CreateAsync(CreateSpecialProductPriceDto dto);
        Task<SpecialProductPriceReadDto?> UpdateAsync(UpdateSpecialProductPriceDto dto);
        Task<bool> DeleteAsync(int id);
        Task<SpecialProductPriceReadDto?> VerifyAsync(int id);
        Task<SpecialProductPriceReadDto?> ApproveAsync(int id);
        Task<SpecialProductPriceReadDto?> SendToAgentAsync(int id);
        Task<SpecialProductPriceReadDto?> AgentApprovalAsync(string approvalKey);
    }
}
