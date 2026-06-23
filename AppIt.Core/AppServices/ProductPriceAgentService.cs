using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Core.AppServices
{
    public class ProductPriceAgentService : IProductPriceAgentService
    {
        private readonly AppItDbContext _context;
        private readonly IEmailNotificationService _email;

        public ProductPriceAgentService(AppItDbContext context, IEmailNotificationService email)
        {
            _context = context;
            _email = email;
        }

        private static AgentProductPriceReadDto Map(AgentProductPrice e) => new()
        {
            Id = e.Id, CompanyId = e.CompanyId, ProductType = e.ProductType, ProductId = e.ProductId,
            ProductName = e.ProductName, NetRate = e.NetRate, RackRate = e.RackRate,
            IsApproved = e.IsApproved, IsAgentApproved = e.IsAgentApproved, IsVerified = e.IsVerified,
            YearEffected = e.YearEffected, ApprovalKey = e.ApprovalKey
        };

        public async Task<IEnumerable<AgentProductPriceReadDto>> GetAllAsync(int? companyId = null, int? year = null)
        {
            var q = _context.AgentProductPrices.AsNoTracking().AsQueryable();
            if (companyId.HasValue) q = q.Where(a => a.CompanyId == companyId);
            if (year.HasValue) q = q.Where(a => a.YearEffected == year);
            return await q.OrderByDescending(a => a.Id).Select(a => Map(a)).ToListAsync();
        }

        public async Task<AgentProductPriceReadDto> CreateAsync(CreateAgentProductPriceDto dto)
        {
            var entity = new AgentProductPrice
            {
                CompanyId = dto.CompanyId, ProductType = dto.ProductType, ProductId = dto.ProductId,
                ProductName = dto.ProductName, NetRate = dto.NetRate, RackRate = dto.RackRate,
                YearEffected = dto.YearEffected ?? DateTime.UtcNow.Year
            };
            _context.AgentProductPrices.Add(entity);
            await _context.SaveChangesAsync();
            return Map(entity);
        }

        public async Task<AgentProductPriceReadDto?> VerifyAsync(int id, string verifiedBy)
        {
            var e = await _context.AgentProductPrices.FindAsync(id);
            if (e == null) return null;
            e.IsVerified = true;
            e.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Map(e);
        }

        public async Task<AgentProductPriceReadDto?> ApproveAsync(int id, string approvedBy)
        {
            var e = await _context.AgentProductPrices.FindAsync(id);
            if (e == null) return null;
            e.IsApproved = true;
            e.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Map(e);
        }

        public async Task<AgentProductPriceReadDto?> SendToAgentAsync(int id)
        {
            var e = await _context.AgentProductPrices.Include(a => a.Agent).FirstOrDefaultAsync(a => a.Id == id);
            if (e == null) return null;
            e.ApprovalKey = Guid.NewGuid().ToString("N");
            e.Sent = true;
            e.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            if (!string.IsNullOrWhiteSpace(e.Agent?.CompanyEmail))
            {
                await _email.SendAsync(e.Agent.CompanyEmail, "Agent rate approval",
                    $"Please approve rates: /special-rates/agent-approval/{e.ApprovalKey}");
            }
            return Map(e);
        }

        public async Task<AgentProductPriceReadDto?> AgentApprovalAsync(AgentApprovalDto dto)
        {
            var e = await _context.AgentProductPrices.FirstOrDefaultAsync(a => a.ApprovalKey == dto.ApprovalKey);
            if (e == null) return null;
            e.IsAgentApproved = true;
            e.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Map(e);
        }
    }
}
