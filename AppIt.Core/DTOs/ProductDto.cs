using AppIt.Data.EntityModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppIt.Core.DTOs
{
    public  class ProductDto
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

    public class CreateProductDto
    {
        public int SupplierId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public decimal ProductPrice { get; set; }
    }
    public class UpdateProductDto
    {
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public decimal ProductPrice { get; set; }
    }

    public class ProductFilterDto
    {
        public int? SupplierId { get; set; }
        public string? ProductName { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }

    public class ProductResponseDto
    {
        public List<ProductDto> Products { get; set; }
        public int TotalCount { get; set; }
    }  
    
    public class SupplierDto
    {
        public int SupplierId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyPhone { get; set; }
    }

    public class CustomerDto
    {
        public int CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
    public class EmployeeDto
    {
        public int EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string JobTitle { get; set; }
    }

    public class OrderDto
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
