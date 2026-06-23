using AppIt.Core.DTOs;

namespace AppIt.Core.Interfaces.Services
{
    public interface IProductPriceAgentService
    {
        Task<IEnumerable<AgentProductPriceReadDto>> GetAllAsync(int? companyId = null, int? year = null);
        Task<AgentProductPriceReadDto> CreateAsync(CreateAgentProductPriceDto dto);
        Task<AgentProductPriceReadDto?> VerifyAsync(int id, string verifiedBy);
        Task<AgentProductPriceReadDto?> ApproveAsync(int id, string approvedBy);
        Task<AgentProductPriceReadDto?> SendToAgentAsync(int id);
        Task<AgentProductPriceReadDto?> AgentApprovalAsync(AgentApprovalDto dto);
    }
}
