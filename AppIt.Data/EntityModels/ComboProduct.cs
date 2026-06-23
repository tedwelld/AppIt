using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Data.EntityModels
{
    public class ComboProduct
    {
        [Key]
        public int Id { get; set; }

        public int ComboId { get; set; }
        public Combo? Combo { get; set; }

        [MaxLength(30)]
        public string ServiceType { get; set; } = "Product";

        public int ServiceId { get; set; }

        [MaxLength(180)]
        public string ServiceName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        public int? SupplierId { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsTaxable { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
