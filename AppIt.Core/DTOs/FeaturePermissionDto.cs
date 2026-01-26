using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Core.DTOs
{
    public class FeaturePermissionReadDto
    {
        public int FeaturePermissionId { get; set; }
        public int FeatureId { get; set; }
        public int PermissionId { get; set; }
    }

    public class CreateFeaturePermissionDto
    {
        [Required]
        public int FeatureId { get; set; }

        [Required]
        public int PermissionId { get; set; }
    }

    public class UpdateFeaturePermissionDto : CreateFeaturePermissionDto
    {
        [Required]
        public int FeaturePermissionId { get; set; }
    }
}
