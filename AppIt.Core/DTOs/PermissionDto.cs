using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Core.DTOs
{
    public class PermissionReadDto
    {
        public int PermissionId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class CreatePermissionDto
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;
    }

    public class UpdatePermissionDto : CreatePermissionDto
    {
        [Required]
        public int PermissionId { get; set; }
    }
}
