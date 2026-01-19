using AppIt.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppIt.Core.Interfaces
{
    public interface ICompanyService
    {
        Task<ServiceResponse<bool>> CompanyEnabledAsync(int companyId, int featureId);
        Task<ServiceResponse<UpdateCompanyDto>> UpdateCompanyAsync(int companyId, int featureId, int permissionId);
        Task<ServiceResponse<CreateCompanyDto>> CreatCompanyAsync(int companyId, int featureId, int permissionId);
        Task<ServiceResponse<CompanyDetailsDto>> CompanyDetailAsync(); 
        Task<ServiceResponse<List<CompanyListDto>>> CompanyListAsync( int companyId);
        Task<ServiceResponse<List<CompanyDropdownDto>>> CompanyDropdownAsync(int companyId);
        Task<ServiceResponse<bool>> DeleteCompanyAsync(int compayId, int featureId);
        Task<ServiceResponse<List<CompanySummaryDto>>> CompanySummary(int companyId);
    }
}
