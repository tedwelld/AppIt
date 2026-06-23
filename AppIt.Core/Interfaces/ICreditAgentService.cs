using System.Threading.Tasks;

namespace AppIt.Core.Interfaces.Services
{
    public interface ICreditAgentService
    {
        Task ValidateCreditLimitAsync(int? companyId, decimal additionalAmount);
        Task<decimal> GetAvailableCreditAsync(int companyId);
        Task<decimal> GetOutstandingBalanceAsync(int companyId);
        bool IsCreditAgent(int? companyId, string? agentType);
    }
}
