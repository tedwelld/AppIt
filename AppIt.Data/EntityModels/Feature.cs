using System;
using System.Collections.Generic;
using System.Text;
using AppIt.Data.AggregateRoots;


namespace AppIt.Data.EntityModels
{
    public  class Feature : FullAuditedAggregateRoot<int>
    {
        public int? PermissionId { get; set; }
        public Permission? Permission { get; set; }
        public string Description { get; set; } = string.Empty;

        public ICollection<FeaturePermission> FeaturePermissions { get; set; } = new List<FeaturePermission>();
        public ICollection<RoleFeature> RoleFeatures { get; set; } = new List<RoleFeature>();
        public ICollection<RoleFeaturePermission> RoleFeaturePermissions { get; set; } = new List<RoleFeaturePermission>();
    }
}
