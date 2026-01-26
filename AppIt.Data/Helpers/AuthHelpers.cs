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
                    int parsedId;
                    if (!int.TryParse(featureIds, out parsedId))
                    {
                        // try to resolve from enum name
                        if (!Enum.TryParse(typeof(Data.Enums.FeatureId), featureIds, true, out var enumVal) || enumVal == null)
                        {
                            continue; // skip unknown identifiers
                        }
                        parsedId = (int)enumVal;
                    }

                    featurePermissions.Add(new FeaturePermission
                    {
                        Feature = feature,
                        Permission = permission,
                        FeatureId = parsedId,
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
                foreach (var featureIdStr in featureAttributes.FeatureIds)
                {
                    int parsedId;
                    if (!int.TryParse(featureIdStr, out parsedId))
                    {
                        if (!Enum.TryParse(typeof(Data.Enums.FeatureId), featureIdStr, true, out var enumVal) || enumVal == null)
                        {
                            continue;
                        }
                        parsedId = (int)enumVal;
                    }

                    roleFeaturePermissions.Add(new RoleFeaturePermission
                    {
                        RoleId = roleId,
                        Feature = feature,
                        Permission = permission,
                        FeatureId = parsedId,
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
                foreach (var featureIdStr in featureAttributes.FeatureIds)
                {
                    int parsedId;
                    if (!int.TryParse(featureIdStr, out parsedId))
                    {
                        if (!Enum.TryParse(typeof(Data.Enums.FeatureId), featureIdStr, true, out var enumVal) || enumVal == null)
                        {
                            continue;
                        }
                        parsedId = (int)enumVal;
                    }

                    roleFeatures.Add(new RoleFeature
                    {
                        RoleId = roleId,
                        Feature = feature,
                        FeatureId = parsedId,
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
                foreach (var featureIdStr in featureAttributes.FeatureIds)
                {
                    int parsedId;
                    if (!int.TryParse(featureIdStr, out parsedId))
                    {
                        if (!Enum.TryParse(typeof(Data.Enums.FeatureId), featureIdStr, true, out var enumVal) || enumVal == null)
                        {
                            continue;
                        }
                        parsedId = (int)enumVal;
                    }

                    roleFeatures.Add(new RoleFeature
                    {
                        RoleId = roleId,
                        Feature = feature,
                        FeatureId = parsedId,
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
                foreach (var featureIdStr in featureAttributes.FeatureIds)
                {
                    int parsedId;
                    if (!int.TryParse(featureIdStr, out parsedId))
                    {
                        if (!Enum.TryParse(typeof(Data.Enums.FeatureId), featureIdStr, true, out var enumVal) || enumVal == null)
                        {
                            continue;
                        }
                        parsedId = (int)enumVal;
                    }

                    featurePermissions.Add(new FeaturePermission
                    {
                        Feature = feature,
                        FeatureId = parsedId
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
                foreach (var featureIdStr in featureAttributes.FeatureIds)
                {
                    int parsedId;
                    if (!int.TryParse(featureIdStr, out parsedId))
                    {
                        if (!Enum.TryParse(typeof(Data.Enums.FeatureId), featureIdStr, true, out var enumVal) || enumVal == null)
                        {
                            continue;
                        }
                        parsedId = (int)enumVal;
                    }

                    roleFeaturePermissions.Add(new RoleFeaturePermission
                    {
                        RoleId = roleId,
                        Feature = feature,
                        FeatureId = parsedId,
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
                foreach (var featureIdStr in featureAttributes.FeatureIds)
                {
                    int parsedId;
                    if (!int.TryParse(featureIdStr, out parsedId))
                    {
                        if (!Enum.TryParse(typeof(Data.Enums.FeatureId), featureIdStr, true, out var enumVal) || enumVal == null)
                        {
                            continue;
                        }
                        parsedId = (int)enumVal;
                    }

                    roleFeaturePermissions.Add(new RoleFeaturePermission
                    {
                        RoleId = roleId,
                        Feature = feature,
                        FeatureId = parsedId,
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
                foreach (var featureIdStr in featureAttributes.FeatureIds)
                {
                    int parsedId;
                    if (!int.TryParse(featureIdStr, out parsedId))
                    {
                        if (!Enum.TryParse(typeof(Data.Enums.FeatureId), featureIdStr, true, out var enumVal) || enumVal == null)
                        {
                            continue;
                        }
                        parsedId = (int)enumVal;
                    }

                    roleFeatures.Add(new RoleFeature
                    {
                        RoleId = roleId,
                        FeatureId = parsedId,
                        IsActivated = true
                    });
                }
            }
            return roleFeatures;


        }
    }
}