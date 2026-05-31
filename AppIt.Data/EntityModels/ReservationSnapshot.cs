using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Data.EntityModels
{
    public class ReservationSnapshot
    {
        [Key]
        public int Id { get; set; }

        public int ReservationId { get; set; }
        public Reservation? Reservation { get; set; }

        [MaxLength(30)]
        public string SnapshotType { get; set; } = "Created";

        public string SnapshotJson { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
