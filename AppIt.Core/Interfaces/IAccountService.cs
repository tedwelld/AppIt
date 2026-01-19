using AppIt.Core.DTOs;
using AppIt.Data.Helpers;
using AppIt.Data.EntityModels;
using AppIt.Data.Enums;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace AppIt.Core.Interfaces
{
    public interface IAccountService
    {
        // Please specify the type parameter for ServiceResponse<T> and the method signature.
        // Example placeholder method:
         //Task<ServiceResponse<>> GetUserAsync(int userId);
        Task<Account> GetAccountAsync(int accountId);
        Task<ServiceResponse<List<AccountResponseDto>>> GetAllAccountsAsync(AccountType? accountType = null);
        Task<ServiceResponse<AccountResponseDto>> CreateAccountAsync(CreateAccountDto createAccountDto);
        Task<ServiceResponse<AccountResponseDto>> UpdateAccountAsync(int accountId, UpdateAccountDto updateAccountDto);
        Task<ServiceResponse<bool>> DeleteAccountAsync(int accountId);
        Task<ServiceResponse<bool>> DeleteAccountAsync(int accountId, AccountType accountType);
        Task<ServiceResponse<bool>> DeleteAccountAsync(int accountId, bool hardDelete);
        Task<ServiceResponse<bool>> DeleteAccountAsync(int accountId, AccountType accountType, bool hardDelete);
        Task<ServiceResponse<bool>> DeleteAccountAsync(int accountId, AccountType? accountType = null, bool hardDelete = false);
        Task<ServiceResponse<AccountResponseDto>> GetById(int id);
        Task<ServiceResponse<List<AccountWithRoleDto>>> GetAllRoles();
        Task<ServiceResponse<FeatureDto>> GetAllFeatures();

    }
}
