using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AppIt.Data.EntityModels
{
    public class FeaturePermission
    {
        
        public int FeaturePermissionId { get; set; }
        [Key]
        public int FeatureId { get; set; }
        public Feature? Feature { get; set; }
        public int PermissionId { get; set; }
        public Permission? Permission { get; set; }
    }
}
