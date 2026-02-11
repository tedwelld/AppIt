using System;
using System.Collections.Generic;

namespace AppIt.Data.EntityModels
{
    public class Role
    {
        public int RoleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<Account> Accounts { get; set; } = new List<Account>();
        public ICollection<RoleFeature> RoleFeatures { get; set; } = new List<RoleFeature>();
        public ICollection<RoleFeaturePermission> RoleFeaturePermissions { get; set; } = new List<RoleFeaturePermission>();
    }
}
