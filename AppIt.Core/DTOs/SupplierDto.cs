using System;
using System.Collections.Generic;
using System.Text;
using AppIt.Data.EntityModels;
using AppIt.Data.AggregateRoots;


namespace AppIt.Core.DTOs
{
    public class SupplierDto
    {
        public int SupplierId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
    }

    public class CreateSupplierDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
    }

    public class UpdateSupplierDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
    }
}

