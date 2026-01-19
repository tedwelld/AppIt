using AppIt.Data.EntityModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppIt.Core.DTOs
{
    public class RoleFeatureDto
    {
        public int RoleId { get; set; }
        public Role? Role { get; set; }
        public bool IsActivated { get; set; }
        public int FeatureId { get; set; }
        public Feature? Feature { get; set; }
    }
    public class RoleFeaturePermissionDto
    {
        public int RoleId { get; set; }
        public Role? Role { get; set; }
        public int FeatureId { get; set; }
        public Feature? Feature { get; set; }
        public bool IsActivated { get; set; }
        public int PermissionId { get; set; }
        public Permission? Permission { get; set; }
    }

    public class RolePermissionDto
    {
        public int RoleId { get; set; }
        public Role? Role { get; set; }
        public Permission? PermissionId { get; set; }
        public Permission? Permission { get; set; }
        public bool IsActivated { get; set; }
    }

    public class DeleteRoleFeaturePermissionDto
    {
        public int FeatureId { get; set; }
        public Feature? Feature { get; set; }
        public Permission? PermissionId { get; set; }
        public Permission? Permission { get; set; }
    }
}
