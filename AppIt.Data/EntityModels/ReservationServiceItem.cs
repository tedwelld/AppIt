using System.ComponentModel.DataAnnotations;

namespace AppIt.Data.EntityModels
{
    public class ReservationServiceItem
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public Reservation? Reservation { get; set; }

        [MaxLength(30)]
        public string ServiceType { get; set; } = "Product";

        public int ServiceId { get; set; }

        [MaxLength(180)]
        public string ServiceName { get; set; } = string.Empty;

        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        [MaxLength(10)]
        public string Currency { get; set; } = "USD";

        public int? SupplierId { get; set; }
        public int? AdultPax { get; set; }
        public int? ChildPax { get; set; }
        public int? CompPax { get; set; }
        public int? Rooms { get; set; }
        public int? Nights { get; set; }

        [MaxLength(200)]
        public string? PickupLocation { get; set; }

        [MaxLength(200)]
        public string? DropoffLocation { get; set; }

        public DateTime? ActivityDate { get; set; }
        public decimal? DiscountPercent { get; set; }
        public decimal? VatPercent { get; set; }
        public decimal? CostOfSale { get; set; }

        [MaxLength(200)]
        public string? Notes { get; set; }
    }
}
