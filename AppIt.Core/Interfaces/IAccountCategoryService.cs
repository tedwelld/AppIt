using AppIt.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AppIt.Core.Interfaces
{
    public interface IAccountCategoryService
    {
        // Define methods related to account category management
        Task<ServiceResponse<FeatureDto>> GetAllFeatures();
        Task<ServiceResponse<AccountCategoryDto>> GetAccountCategoryById(int id);
        Task<ServiceResponse<CreateAccountCategoryDto>> GetAccountCategoryById(int id,string name);
        Task<ServiceResponse<UpdateAccountCategoryDto>> UpdateAccountCategory();
        Task<ServiceResponse<DeleteAccountCategoryDto>> DeleteAccountCategory(int Id); 
    }
}
