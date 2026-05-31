using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Data.EntityModels
{
    public class ProofOfPayment
    {
        [Key]
        public int Id { get; set; }

        public int? PaymentId { get; set; }
        public Payment? Payment { get; set; }

        public int? InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }

        [MaxLength(500)]
        public string DocumentUrl { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public DateTime? VerifiedAt { get; set; }

        [MaxLength(150)]
        public string? VerifiedBy { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
