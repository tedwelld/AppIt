using System;
using System.Collections.Generic;
using System.Text;

namespace AppIt.Data.EntityModels
{
    public  class RoleFeaturePermission
    {
        public int RoleFeaturePermissionId { get; set; }
        public int RoleId { get; set; }
        public Role? Role { get; set; }

        public int FeatureId { get; set; }
        public Feature? Feature { get; set; }
        public bool IsActivated { get; set; }
        public int PermissionId { get; set; }
        public Permission? Permission { get; set; }
    }
}
