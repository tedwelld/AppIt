using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Core.AppServices
{
    public class TrialBalanceService : ITrialBalanceService
    {
        private readonly AppItDbContext _context;

        public TrialBalanceService(AppItDbContext context) => _context = context;

        public async Task<IEnumerable<TrialBalanceLineDto>> GetTrialBalanceAsync(DateTime? asOf = null)
        {
            var lines = await _context.JournalEntryLines.AsNoTracking()
                .Include(l => l.Account)
                .ToListAsync();

            return lines.GroupBy(l => l.FinancialAccountId)
                .Select(g =>
                {
                    var debit = g.Where(x => x.EntryType == "Debit").Sum(x => x.Amount);
                    var credit = g.Where(x => x.EntryType == "Credit").Sum(x => x.Amount);
                    var name = g.First().Account?.Name ?? "Account";
                    return new TrialBalanceLineDto
                    {
                        AccountId = g.Key,
                        AccountName = name,
                        Debit = debit,
                        Credit = credit,
                        Balance = debit - credit
                    };
                }).OrderBy(x => x.AccountName);
        }
    }
}
