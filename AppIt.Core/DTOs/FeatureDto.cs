using System;
using System.Collections.Generic;
using System.Security;
using System.Text;
using AppIt.Data.EntityModels;

namespace AppIt.Core.DTOs
{
    public class FeatureDto
    {
        public int? FeatureId { get; set; }

        public int? PermissionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public ICollection<FeaturePermission>? FeaturePermissions { get; set; } = new List<FeaturePermission>();
        public ICollection<RoleFeature>? RoleFeatures { get; set; } = new List<RoleFeature>();
    }
    public class CreateFeatureDto
    {
        public int? FeatureId { get; set; }
        public int? PermissionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
    public class UpdateFeatureDto
    {
        public int? FeatureId { get; set; }
        public int? PermissionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class FeatureWithPermissionsDto
    {
        public int? FeatureId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
       
    }

}
