using System;
using System.Collections.Generic;
using System.Text;

namespace AppIt.Data.EntityModels
{
    public class Department
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime DateUpdated { get; set; } = DateTime.Now;
    }
}
