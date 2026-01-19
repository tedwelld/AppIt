using AppIt.Data.EntityModels;
using System;
using System.Collections.Generic;
using System.Text;
using AppIt.Data.Enums;
using System.ComponentModel;


namespace AppIt.Data.Helpers
{
    public static class AuthHelpers
    {
        public static List<FeaturePermission> GetFeaturePermissions(Feature feature, Permission permission)
        {
            var featurePermissions = new List<FeaturePermission>();
            var featureAttributes = (FeatureAttributes?)Attribute.GetCustomAttribute(typeof(Feature), typeof(FeatureAttributes));
            if (featureAttributes != null)
            {
                foreach (var featureIds in featureAttributes.FeatureIds)
                {
                    featurePermissions.Add(new FeaturePermission
                    {
                        Feature = feature,
                        Permission = permission,
                        FeatureId = (int)featureIds,
                    });
                }
            }
            return featurePermissions;
        }

        public static List<RoleFeaturePermission> GetRoleFeaturePermissions(int roleId, Feature feature, Permission permission, bool isActivated, FeatureId featureIds)
        {
            var roleFeaturePermissions = new List<RoleFeaturePermission>();
            var featureAttributes = (FeatureAttributes?)Attribute.GetCustomAttribute(typeof(Feature), typeof(FeatureAttributes));
            if (featureAttributes != null)
            {
                foreach (var featureId in featureAttributes.FeatureIds)
                {
                    roleFeaturePermissions.Add(new RoleFeaturePermission
                    {
                        RoleId = roleId,
                        Feature = feature,
                        Permission = permission,
                        //FeatureId = (int)featureIds,
                        IsActivated = isActivated
                    });
                }
            }
            return roleFeaturePermissions;
        }
        public static List<RoleFeature> GetRoleFeatures(int roleId, Feature feature, bool isActivated)
        {
            var roleFeatures = new List<RoleFeature>();
            var featureAttributes = (FeatureAttributes?)Attribute.GetCustomAttribute(typeof(Feature), typeof(FeatureAttributes));
            if (featureAttributes != null)
            {
                foreach (var featureIds in featureAttributes.FeatureIds)
                {
                    roleFeatures.Add(new RoleFeature
                    {
                        RoleId = roleId,
                        Feature = feature,
                        //FeatureId = (int)featureId,
                        IsActivated = isActivated
                    });
                }
            }
            return roleFeatures;
        }
        public static List<RoleFeature> GetRoleFeatures(int roleId, Feature feature)
        {
            var roleFeatures = new List<RoleFeature>();
            var featureAttributes = (FeatureAttributes?)Attribute.GetCustomAttribute(typeof(Feature), typeof(FeatureAttributes));
            if (featureAttributes != null)
            {
                foreach (var featureId in featureAttributes.FeatureIds)
                {
                    roleFeatures.Add(new RoleFeature
                    {
                        RoleId = roleId,
                        Feature = feature,
                        // FeatureId = (int)featureId,
                        IsActivated = true
                    });
                }
            }
            return roleFeatures;
        }
        public static List

            <FeaturePermission> GetFeaturePermissions(Feature feature)
        {
            var featurePermissions = new List<FeaturePermission>();
            var featureAttributes = (FeatureAttributes?)Attribute.GetCustomAttribute(typeof(Feature), typeof(FeatureAttributes));
            if (featureAttributes != null)
            {
                foreach (var featureId in featureAttributes.FeatureIds)
                {
                    featurePermissions.Add(new FeaturePermission
                    {
                        Feature = feature,
                        FeatureId = (int)featureId
                    });
                }
            }
            return featurePermissions;
        }
        public static List<RoleFeaturePermission> GetRoleFeaturePermissions(int roleId, Feature feature, bool isActivated)
        {
            var roleFeaturePermissions = new List<RoleFeaturePermission>();
            var featureAttributes = (FeatureAttributes?)Attribute.GetCustomAttribute(typeof(Feature), typeof(FeatureAttributes));
            if (featureAttributes != null)
            {
                foreach (var featureId in featureAttributes.FeatureIds)
                {
                    roleFeaturePermissions.Add(new RoleFeaturePermission
                    {
                        RoleId = roleId,
                        Feature = feature,
                        //  FeatureId = (int)featureId,
                        IsActivated = isActivated
                    });
                }
            }
            return roleFeaturePermissions;
        }

        public static List<RoleFeaturePermission> GetRoleFeaturePermissions(int roleId, Feature feature)
        {
            var roleFeaturePermissions = new List<RoleFeaturePermission>();
            var featureAttributes = (FeatureAttributes?)Attribute.GetCustomAttribute(typeof(Feature), typeof(FeatureAttributes));
            if (featureAttributes != null)
            {
                foreach (var featureId in featureAttributes.FeatureIds)
                {
                    roleFeaturePermissions.Add(new RoleFeaturePermission
                    {
                        RoleId = roleId,
                        Feature = feature,
                        // FeatureId = (int)featureId,
                        IsActivated = true
                    });
                }
            }
            return roleFeaturePermissions;
        }
        public static List<RoleFeature> GetRoleFeatures(int roleId)
        {
            var roleFeatures = new List<RoleFeature>();
            var featureAttributes = (FeatureAttributes?)Attribute.GetCustomAttribute(typeof(Feature), typeof(FeatureAttributes));
            if (featureAttributes != null)
            {
                foreach (var featureId in featureAttributes.FeatureIds)
                {
                    roleFeatures.Add(new RoleFeature
                    {
                        RoleId = roleId,
                        // FeatureId = (int)featureId,
                        IsActivated = true
                    });
                }
            }
            return roleFeatures;


        }
    }
}