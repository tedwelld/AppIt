using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Core.DTOs
{
    public class ProofOfPaymentReadDto
    {
        public int Id { get; set; }
        public int? PaymentId { get; set; }
        public int? InvoiceId { get; set; }
        public string DocumentUrl { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime UploadedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? VerifiedBy { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateProofOfPaymentDto
    {
        public int? PaymentId { get; set; }
        public int? InvoiceId { get; set; }
        [Required] public string DocumentUrl { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class UpdateProofOfPaymentDto : CreateProofOfPaymentDto
    {
        [Required] public int Id { get; set; }
        public string Status { get; set; } = "Pending";
    }
}
