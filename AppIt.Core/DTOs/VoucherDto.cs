using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Core.DTOs
{
    public class VoucherReadDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Type { get; set; } = "Reservation";
        public string? ComboReference { get; set; }
        public int? ReservationId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateVoucherDto
    {
        [Required]
        public string Code { get; set; } = string.Empty;
        [Required]
        public string Reference { get; set; } = string.Empty;
        public string Type { get; set; } = "Reservation";
        public string? ComboReference { get; set; }
        public int? ReservationId { get; set; }
    }

    public class UpdateVoucherDto : CreateVoucherDto
    {
        [Required]
        public int Id { get; set; }
    }
}
