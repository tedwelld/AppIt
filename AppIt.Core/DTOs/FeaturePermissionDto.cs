using AppIt.Data.EntityModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppIt.Core.DTOs
{
    public class FeaturePermissionDto
    {
        public int FeatureId { get; set; }
        public Feature? Feature { get; set; }
        public int PermissionId { get; set; }
        public Permission? Permission { get; set; }
    }

    public class CreateFeaturePermissionDto
    {
        public int FeatureId { get; set; }
        public int PermissionId { get; set; }
    }
    public class  UpdateFeaturePermission
    {
        public int FeatureId { get; set; }
        public Feature? Feature { get; set; }
        public int PermissionId { get; set; }
        public Permission? Permission { get; set; }
    }

    public class DeleteFeaturePermissionDto
    {
        public int FeatureId { get; set; }
        public int PermissionId { get; set; }
    }
}
