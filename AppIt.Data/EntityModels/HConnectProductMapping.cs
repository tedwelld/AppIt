using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Data.EntityModels
{
    public class HConnectProductMapping
    {
        [Key]
        public int Id { get; set; }

        public int AccommodationId { get; set; }
        public Accommodation? Accommodation { get; set; }

        [MaxLength(50)]
        public string HConnectRoomCode { get; set; } = string.Empty;

        [MaxLength(50)]
        public string HConnectPropertyCode { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
