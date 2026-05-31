using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Core.DTOs
{
    public class TransferReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<ServicePriceReadDto> Prices { get; set; } = new();
    }

    public class CreateTransferDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateTransferDto : CreateTransferDto
    {
        [Required]
        public int Id { get; set; }
    }
}
