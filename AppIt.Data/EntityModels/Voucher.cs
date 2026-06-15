using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Data.EntityModels
{
    public class Voucher
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Reference { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Type { get; set; } = "Reservation";

        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        [MaxLength(50)]
        public string? ComboReference { get; set; }

        public int? ReservationId { get; set; }
        public Reservation? Reservation { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? RedeemedDate { get; set; }
    }
}
