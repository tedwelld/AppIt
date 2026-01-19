using System;
using System.Collections.Generic;
using System.Text;
using AppIt.Data.AggregateRoots;


namespace AppIt.Data.EntityModels
{
    public  class Feature : FullAuditedAggregateRoot<int>
    {
        public Permission? Permission { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public ICollection<FeaturePermission>? FeaturePermissions { get; set; } = new List<FeaturePermission>();
        public ICollection<RoleFeature>? RoleFeatures { get; set; } = new List<RoleFeature>();
    }
}
