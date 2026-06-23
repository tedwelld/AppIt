using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppIt.Data.EntityModels
{
    public class ReservationServiceItemSplit
    {
        [Key]
        public int Id { get; set; }

        public int ReservationServiceItemId { get; set; }
        public ReservationServiceItem? ReservationServiceItem { get; set; }

        [MaxLength(30)]
        public string ServiceType { get; set; } = "Product";

        public int ServiceId { get; set; }

        [MaxLength(180)]
        public string ServiceName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(12, 2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(12, 2)")]
        public decimal TotalPrice { get; set; }

        public int Quantity { get; set; } = 1;
        public bool IsMandatory { get; set; }
    }
}
