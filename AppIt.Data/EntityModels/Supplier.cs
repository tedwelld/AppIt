using System.Collections.Generic;

namespace AppIt.Data.EntityModels
{
    public class Supplier
    {
        public int SupplierId { get; set; }       // PK
        public string Name { get; set; } = null!;
        public string? Description { get; set; }  // optional
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }

        // Navigation property for products
        //public ICollection<Product>? Products { get; set; }
    }
}
