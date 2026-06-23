using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Core.AppServices
{
    public class CreditAgentService : ICreditAgentService
    {
        private readonly AppItDbContext _context;

        public CreditAgentService(AppItDbContext context) => _context = context;

        public bool IsCreditAgent(int? companyId, string? agentType) =>
            string.Equals(agentType, "Credit", StringComparison.OrdinalIgnoreCase)
            || (companyId.HasValue && _context.Companies.AsNoTracking()
                .Any(c => c.CompanyId == companyId && c.IsCreditAgent));

        public async Task ValidateCreditLimitAsync(int? companyId, decimal additionalAmount)
        {
            if (!companyId.HasValue) return;
            var company = await _context.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.CompanyId == companyId);
            if (company == null || !company.IsCreditAgent) return;

            var outstanding = await GetOutstandingBalanceAsync(companyId.Value);
            if (outstanding + additionalAmount > company.CreditLimit)
                throw new InvalidOperationException("Credit limit exceeded for this agent.");
        }

        public async Task<decimal> GetAvailableCreditAsync(int companyId)
        {
            var company = await _context.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.CompanyId == companyId);
            if (company == null) return 0;
            var outstanding = await GetOutstandingBalanceAsync(companyId);
            return Math.Max(0, company.CreditLimit - outstanding);
        }

        public async Task<decimal> GetOutstandingBalanceAsync(int companyId)
        {
            return await _context.Reservations.AsNoTracking()
                .Where(r => r.AgencyId == companyId
                    && r.Status != "Cancelled"
                    && r.Status != "Closed"
                    && r.PaymentStatus != "FullyPaid")
                .SumAsync(r => (decimal?)r.TotalAmount) ?? 0m;
        }
    }
}
