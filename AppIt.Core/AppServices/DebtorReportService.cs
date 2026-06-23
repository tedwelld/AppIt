using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Core.AppServices
{
    public class DebtorReportService : IDebtorReportService
    {
        private readonly AppItDbContext _context;
        private readonly ICreditAgentService _credit;

        public DebtorReportService(AppItDbContext context, ICreditAgentService credit)
        {
            _context = context;
            _credit = credit;
        }

        public async Task<IEnumerable<DebtorLineDto>> GetAgingAsync(int? companyId = null)
        {
            var companies = await _context.Companies.AsNoTracking()
                .Where(c => c.IsCreditAgent || c.AgentType == "Credit")
                .Where(c => !companyId.HasValue || c.CompanyId == companyId)
                .ToListAsync();

            var result = new List<DebtorLineDto>();
            foreach (var c in companies)
            {
                result.Add(new DebtorLineDto
                {
                    CompanyId = c.CompanyId,
                    CompanyName = c.CompanyName,
                    CreditLimit = c.CreditLimit,
                    Outstanding = await _credit.GetOutstandingBalanceAsync(c.CompanyId)
                });
            }
            return result;
        }
    }
}
