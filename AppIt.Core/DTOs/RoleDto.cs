using System;
using System.Collections.Generic;
using System.Text;

namespace AppIt.Core.DTOs
{
    public class RoleDto
    {
        public string Name { get; set; } = string.Empty;
    }
    public class RoleWithFeaturesDto
    {
        public string Name { get; set; } = string.Empty;
        public ICollection<FeatureDto>? Features { get; set; } = new List<FeatureDto>();
    }

    public class DeleteDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<PermissionDto>? Permissions { get; set; } = new List<PermissionDto>();
    }
}
