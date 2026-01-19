using AppIt.Data.EntityModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppIt.Core.DTOs
{
    public class PermissionDto
    {
        public string Name { get; set; } = string.Empty;

        public ICollection<FeaturePermission>? FeaturePermissions { get; set; } = new List<FeaturePermission>();
    }

    public class CreatePermissionDto
    {
        public string Name { get; set; } = string.Empty;
    }
    public class UpdatePermissionDto
    {
        public string Name { get; set; } = string.Empty;
    }
    public class DeletePermissionDto
    { 
        public int Id { get; set; }
    }
    public class GetPermissionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<FeaturePermission>? FeaturePermissions { get; set; } = new List<FeaturePermission>();
    }

}
