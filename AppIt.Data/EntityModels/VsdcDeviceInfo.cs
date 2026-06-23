using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Data.EntityModels
{
    public class VsdcDeviceInfo
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(100)]
        public string DeviceId { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? SdcId { get; set; }

        [MaxLength(100)]
        public string? MrcNo { get; set; }

        public bool IsInitialized { get; set; }
        public DateTime? InitializedAt { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
