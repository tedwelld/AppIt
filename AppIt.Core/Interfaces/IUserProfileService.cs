using AppIt.Core.DTOs;
using AppIt.Core.DTOs.AppIt.Core.DTOs;
using AppIt.Core.DTOs.AppIt.Core.DTOs.AppIt.Core.DTOs;
using AppIt.Data;
using AppIt.Data.EntityModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppIt.Core.Interfaces
{
    public interface IUserProfileService
    {
        Task<DTOs.AppIt.Core.DTOs.UserProfileReadDto> CreateAsync(CreateUserProfileDto dto);
    }


}
