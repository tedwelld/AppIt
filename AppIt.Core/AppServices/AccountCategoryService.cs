using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using System.Threading.Tasks;

namespace AppIt.Core.AppServices
{
    public class AccountCategoryService : IAccountCategoryService
    {
        public Task<ServiceResponse<AccountCategoryDto>> GetAccountCategoryById(int id)
        {
            return Task.FromResult(new ServiceResponse<AccountCategoryDto>(new AccountCategoryDto(), "Account category retrieved (stub)"));
        }

        public Task<ServiceResponse<CreateAccountCategoryDto>> GetAccountCategoryById(int id, string name)
        {
            return Task.FromResult(new ServiceResponse<CreateAccountCategoryDto>(new CreateAccountCategoryDto(), "Account category retrieved (stub)"));
        }

        public Task<ServiceResponse<FeatureDto>> GetAllFeatures()
        {
            return Task.FromResult(new ServiceResponse<FeatureDto>(new FeatureDto(), "Features retrieved (stub)"));
        }

        public Task<ServiceResponse<UpdateAccountCategoryDto>> UpdateAccountCategory()
        {
            return Task.FromResult(new ServiceResponse<UpdateAccountCategoryDto>(new UpdateAccountCategoryDto(), "Account category updated (stub)"));
        }

        public Task<ServiceResponse<DeleteAccountCategoryDto>> DeleteAccountCategory(int Id)
        {
            return Task.FromResult(new ServiceResponse<DeleteAccountCategoryDto>(new DeleteAccountCategoryDto(), "Account category deleted (stub)"));
        }
    }
}
