using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Core.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly AppItDbContext _context;

        public DepartmentService(AppItDbContext context)
        {
            _context = context;
        }

        public async Task<DepartmentReadDto> CreateAsync(CreateDepartmentDto dto)
        {
            var department = new Department
            {
                Name = dto.Name,
                Description = dto.Description,
                UpdatedBy = dto.UpdatedBy,
                DateUpdated = DateTime.UtcNow
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            return ToReadDto(department);
        }

        public async Task<DepartmentReadDto?> UpdateAsync(UpdateDepartmentDto dto)
        {
            var department = await _context.Departments.FindAsync(dto.Id);
            if (department == null) return null;

            department.Name = dto.Name;
            department.Description = dto.Description;
            department.UpdatedBy = dto.UpdatedBy;
            department.DateUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return ToReadDto(department);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null) return false;

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<DepartmentReadDto?> GetByIdAsync(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            return department == null ? null : ToReadDto(department);
        }

        public async Task<IEnumerable<DepartmentReadDto>> GetAllAsync()
        {
            var departments = await _context.Departments.ToListAsync();
            return departments.Select(ToReadDto);
        }

        private DepartmentReadDto ToReadDto(Department d) => new()
        {
            Id = d.Id,
            Name = d.Name,
            Description = d.Description,
            UpdatedBy = d.UpdatedBy,
            DateUpdated = d.DateUpdated
        };
    }
}
