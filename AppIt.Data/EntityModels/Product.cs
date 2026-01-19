using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AppIt.Data.EntityModels
{
    public  class Product
    {
        public int SupplierId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
         public Company? Supplier { get; set; }
        public int ProductId { get; set; } 
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public decimal ProductPrice { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
       
         

    }
}
