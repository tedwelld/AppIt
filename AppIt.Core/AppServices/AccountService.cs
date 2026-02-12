using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppIt.Core.AppServices
{
    public class AccountService : IAccountService
    {
        private readonly AppItDbContext _db;

        public AccountService(AppItDbContext db)
        {
            _db = db;
        }

        public async Task<ServiceResponse<AccountDto>> CreateAsync(CreateAccountDto dto)
        {
            var roleId = await ResolveRoleIdAsync(dto.RoleId, dto.Role);
            if (roleId == null)
            {
                return new ServiceResponse<AccountDto>
                {
                    Success = false,
                    Message = "Invalid role"
                };
            }

            var account = new Account
            {
                
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                AvatarUrl = dto.AvatarUrl,
                PreferredCurrency = string.IsNullOrWhiteSpace(dto.PreferredCurrency) ? "USD" : dto.PreferredCurrency,
              
                RoleId = roleId.Value,
              
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _db.Accounts.Add(account);
            await _db.SaveChangesAsync();

            return new ServiceResponse<AccountDto>
            {
                Data = MapToDto(account),
                Success = true,
                Message = "Account created"
            };
        }

        public async Task<ServiceResponse<List<AccountDto>>> GetAllAsync()
        {
            var accounts = await _db.Accounts
                .AsNoTracking()
                .Include(a => a.Role)
                .ToListAsync();

            var dtos = accounts.Select(MapToDto).ToList();
            return new ServiceResponse<List<AccountDto>>(dtos, "Accounts retrieved");
        }

        public async Task<ServiceResponse<AccountDto>> GetByIdAsync(int id)
        {
            var account = await _db.Accounts
                .AsNoTracking()
                .Include(a => a.Role)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (account == null)
                return new ServiceResponse<AccountDto>(null, "Not found") { Success = false };

            return new ServiceResponse<AccountDto>(MapToDto(account), "Retrieved");
        }

        public async Task<ServiceResponse<AccountDto>> UpdateAsync(int id, UpdateAccountDto dto)
        {
            var account = await _db.Accounts.FindAsync(id);
            if (account == null)
                return new ServiceResponse<AccountDto>(null, "Not found") { Success = false };

            var roleId = await ResolveRoleIdAsync(dto.RoleId, dto.Role);
            if (roleId == null)
            {
                return new ServiceResponse<AccountDto>(null, "Invalid role") { Success = false };
            }

            account.FirstName = dto.FirstName;
            account.LastName = dto.LastName;
            account.Email = dto.Email;
            account.Phone = dto.Phone;
            account.AvatarUrl = dto.AvatarUrl;
            account.PreferredCurrency = string.IsNullOrWhiteSpace(dto.PreferredCurrency) ? "USD" : dto.PreferredCurrency;
            account.RoleId = roleId.Value;
         
            account.IsActive = dto.IsActive;
            account.UpdatedDate = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return new ServiceResponse<AccountDto>(MapToDto(account), "Updated");
        }

        public async Task<ServiceResponse<bool>> DeleteAsync(int id)
        {
            var account = await _db.Accounts.FindAsync(id);
            if (account == null)
                return new ServiceResponse<bool>(false, "Not found") { Success = false };

            _db.Accounts.Remove(account);
            await _db.SaveChangesAsync();

            return new ServiceResponse<bool>(true, "Deleted");
        }

        private static AccountDto MapToDto(Account a) => new AccountDto
        {
            Id = a.Id,
           
            FirstName = a.FirstName,
            LastName = a.LastName,
            Email = a.Email,
            Phone = a.Phone,
            AvatarUrl = a.AvatarUrl,
            PreferredCurrency = a.PreferredCurrency,
            Role = a.Role?.Name ?? "regular",
            RoleId = a.RoleId,
           
            IsActive = a.IsActive,
            CreatedDate = a.CreatedDate,
            UpdatedDate = a.UpdatedDate
        };

        private async Task<int?> ResolveRoleIdAsync(int? roleId, string? roleName)
        {
            if (roleId.HasValue)
            {
                var exists = await _db.Roles.AnyAsync(r => r.RoleId == roleId.Value);
                return exists ? roleId.Value : null;
            }

            if (string.IsNullOrWhiteSpace(roleName))
            {
                return null;
            }

            var normalized = roleName.Trim().ToLowerInvariant();
            var existing = await _db.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == normalized);
            if (existing != null)
            {
                return existing.RoleId;
            }

            var role = new Role { Name = normalized };
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();
            return role.RoleId;
        }
    }
}
