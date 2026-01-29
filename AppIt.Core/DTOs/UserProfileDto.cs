using AppIt.Data.EntityModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppIt.Core.DTOs
{
    namespace AppIt.Core.DTOs
    {
        namespace AppIt.Core.DTOs
        {
            public record CreateUserProfileDto(string DisplayName);

            public record UserProfileReadDto(
                int Id,
                string? DisplayName,
                bool IsActive
            );
        }


        public record UserProfileReadDto(
            int Id,
          
            string DisplayName,
            bool IsActive
        );
        
    }

}
