using AppIt.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppIt.Core.Interfaces
{
    public interface IFeatureService
    {
        Task<ServiceResponse<FeatureDto>> GetFeatureAsync();
        Task<ServiceResponse<FeatureDto>> GetFeatureByIdAsync(int id);
        Task<ServiceResponse<CreateFeatureDto>> CreateFeatureAsync(CreateFeatureDto createDto);
        Task<ServiceResponse<UpdateFeatureDto>> UpdateFeatureAsync(int id, UpdateFeatureDto updateDto);
        Task<ServiceResponse<bool>> DeleteFeatureAsync(int id);

    }
}
