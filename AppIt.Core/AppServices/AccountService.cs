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
            if (!await _db.Roles.AnyAsync(r => r.RoleId == dto.RoleId))
            {
                return new ServiceResponse<AccountDto>
                {
                    Success = false,
                    Message = "Invalid RoleId"
                };
            }

            var account = new Account
            {
                
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                NationalId = dto.NationalId,
                Email = dto.Email,
              
                RoleId = dto.RoleId,
              
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
                .Select(a => MapToDto(a))
                .ToListAsync();

            return new ServiceResponse<List<AccountDto>>(accounts, "Accounts retrieved");
        }

        public async Task<ServiceResponse<AccountDto>> GetByIdAsync(int id)
        {
            var account = await _db.Accounts.FindAsync(id);
            if (account == null)
                return new ServiceResponse<AccountDto>(null, "Not found") { Success = false };

            return new ServiceResponse<AccountDto>(MapToDto(account), "Retrieved");
        }

        public async Task<ServiceResponse<AccountDto>> UpdateAsync(int id, UpdateAccountDto dto)
        {
            var account = await _db.Accounts.FindAsync(id);
            if (account == null)
                return new ServiceResponse<AccountDto>(null, "Not found") { Success = false };

            
            account.FirstName = dto.FirstName;
            account.LastName = dto.LastName;
            account.NationalId = dto.NationalId;
            account.Email = dto.Email;
            account.RoleId = dto.RoleId;
         
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
            NationalId = a.NationalId,
            Email = a.Email,
            RoleId = a.RoleId,
           
            IsActive = a.IsActive,
            CreatedDate = a.CreatedDate,
            UpdatedDate = a.UpdatedDate
        };
    }
}
