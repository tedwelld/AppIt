using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Core.DTOs
{
    public class FeatureReadDto
    {
        public int Id { get; set; }

        public int? PermissionId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }

    public class CreateFeatureDto
    {
        public int? PermissionId { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
    }

    public class UpdateFeatureDto : CreateFeatureDto
    {
        [Required]
        public int Id { get; set; }
    }
}
