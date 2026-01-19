using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using AppIt.Data.EntityModels;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AppIt.Core.AppServices
{
    public class FeatureService : IFeatureService
    {
        public Task<ServiceResponse<CreateFeatureDto>> CreateFeatureAsync(CreateFeatureDto createDto)
        {
            return Task.FromResult(new ServiceResponse<CreateFeatureDto>(createDto, "Feature created (stub)"));
        }

        public Task<ServiceResponse<FeatureDto>> GetFeatureAsync()
        {
            return Task.FromResult(new ServiceResponse<FeatureDto>(new FeatureDto(), "Features retrieved (stub)"));
        }

        public Task<ServiceResponse<FeatureDto>> GetFeatureByIdAsync(int id)
        {
            return Task.FromResult(new ServiceResponse<FeatureDto>(new FeatureDto(), "Feature retrieved (stub)"));
        }

        public Task<ServiceResponse<UpdateFeatureDto>> UpdateFeatureAsync(int id, UpdateFeatureDto updateDto)
        {
            return Task.FromResult(new ServiceResponse<UpdateFeatureDto>(updateDto, "Feature updated (stub)"));
        }

        public Task<ServiceResponse<bool>> DeleteFeatureAsync(int id)
        {
            return Task.FromResult(new ServiceResponse<bool>(true, "Feature deleted (stub)"));
        }
    }
}
