using System;
using System.Collections.Generic;
using System.Text;

namespace AppIt.Data.EntityModels
{
    public  class Permission
    {
        public int PermissionId { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<FeaturePermission> FeaturePermissions { get; set; } = new List<FeaturePermission>();
        public ICollection<RoleFeaturePermission> RoleFeaturePermissions { get; set; } = new List<RoleFeaturePermission>();
        public ICollection<Feature> Features { get; set; } = new List<Feature>();
    }
}
