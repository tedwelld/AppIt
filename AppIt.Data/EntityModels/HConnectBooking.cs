using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AppIt.Data.EntityModels
{
    public class HConnectBooking
    {
        [Key]
        public int Id { get; set; }

        public int ReservationId { get; set; }
        public Reservation? Reservation { get; set; }

        [MaxLength(50)]
        public string HConnectPropertyCode { get; set; } = string.Empty;

        [MaxLength(50)]
        public string HConnectRoomCode { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? HConnectConfirmationNumber { get; set; }

        [MaxLength(30)]
        public string SyncStatus { get; set; } = "Pending";

        public DateOnly ArrivalDate { get; set; }
        public DateOnly DepartureDate { get; set; }

        [MaxLength(150)]
        public string GuestFirstName { get; set; } = string.Empty;

        [MaxLength(150)]
        public string GuestLastName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(12, 2)")]
        public decimal TotalAmount { get; set; }

        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }

        public DateTime? LastSyncAttempt { get; set; }
        public int RetryCount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
