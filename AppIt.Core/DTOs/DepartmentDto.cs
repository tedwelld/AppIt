using System;
using System.Collections.Generic;
using System.Text;

namespace AppIt.Core.DTOs
{
    public class DepartmentDto
    {

        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime DateUpdated { get; set; } = DateTime.Now;
    }
    public class CreateDepartmentDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int UpdatedBy { get; set; }
    }
    public class UpdateDepartmentDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int UpdatedBy { get; set; }
    }
    public class DeleteDepartmentDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
    public class GetDepartmentDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
    public class DepartmentFilterDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

    public class DepartmentResponseDto
    {
        public List<DepartmentDto> Departments { get; set; } = new List<DepartmentDto>();
        public int TotalCount { get; set; }
    }

}
