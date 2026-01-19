using System;
using System.Collections.Generic;
using System.Text;

namespace AppIt.Data.EntityModels
{
    public  class Company
    {
        public int  CompanyId { get; set; }
        public string CompanyName { get; set; } 
        public string CompanyAddress { get; set; } 
        public string CompanyEmail { get; set; } 
        public string CompanyPhone { get; set; } 
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
         public string RegNumber { get; set; } = string.Empty;
         public string AccountNumber { get; set; } 
         public string VatNumber { get; set; }
        

    }
}
