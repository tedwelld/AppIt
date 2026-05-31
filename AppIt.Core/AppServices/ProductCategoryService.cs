using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppIt.Core.AppServices
{
    public class ProductCategoryService : IProductCategoryService
    {
        private readonly AppItDbContext _context;

        public ProductCategoryService(AppItDbContext context) => _context = context;

        private static ProductCategoryReadDto Map(ProductCategory c) => new()
        {
            Id = c.Id, Name = c.Name, Description = c.Description, IsActive = c.IsActive, CreatedAt = c.CreatedAt
        };

        public async Task<IEnumerable<ProductCategoryReadDto>> GetAllAsync() =>
            await _context.ProductCategories.AsNoTracking().OrderBy(c => c.Name)
                .Select(c => new ProductCategoryReadDto
                {
                    Id = c.Id, Name = c.Name, Description = c.Description, IsActive = c.IsActive, CreatedAt = c.CreatedAt
                }).ToListAsync();

        public async Task<ProductCategoryReadDto?> GetByIdAsync(int id)
        {
            var c = await _context.ProductCategories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return c == null ? null : Map(c);
        }

        public async Task<ProductCategoryReadDto> CreateAsync(CreateProductCategoryDto dto)
        {
            var entity = new ProductCategory { Name = dto.Name, Description = dto.Description, IsActive = dto.IsActive };
            _context.ProductCategories.Add(entity);
            await _context.SaveChangesAsync();
            return Map(entity);
        }

        public async Task<ProductCategoryReadDto?> UpdateAsync(UpdateProductCategoryDto dto)
        {
            var entity = await _context.ProductCategories.FindAsync(dto.Id);
            if (entity == null) return null;
            entity.Name = dto.Name; entity.Description = dto.Description; entity.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();
            return Map(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.ProductCategories.FindAsync(id);
            if (entity == null) return false;
            _context.ProductCategories.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }

    public class ProductSubCategoryService : IProductSubCategoryService
    {
        private readonly AppItDbContext _context;

        public ProductSubCategoryService(AppItDbContext context) => _context = context;

        private static ProductSubCategoryReadDto Map(ProductSubCategory c) => new()
        {
            Id = c.Id, CategoryId = c.CategoryId, Name = c.Name, Description = c.Description, IsActive = c.IsActive, CreatedAt = c.CreatedAt
        };

        public async Task<IEnumerable<ProductSubCategoryReadDto>> GetAllAsync() =>
            await _context.ProductSubCategories.AsNoTracking().OrderBy(c => c.Name)
                .Select(c => new ProductSubCategoryReadDto
                {
                    Id = c.Id, CategoryId = c.CategoryId, Name = c.Name, Description = c.Description, IsActive = c.IsActive, CreatedAt = c.CreatedAt
                }).ToListAsync();

        public async Task<ProductSubCategoryReadDto?> GetByIdAsync(int id)
        {
            var c = await _context.ProductSubCategories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return c == null ? null : Map(c);
        }

        public async Task<ProductSubCategoryReadDto> CreateAsync(CreateProductSubCategoryDto dto)
        {
            var entity = new ProductSubCategory { CategoryId = dto.CategoryId, Name = dto.Name, Description = dto.Description, IsActive = dto.IsActive };
            _context.ProductSubCategories.Add(entity);
            await _context.SaveChangesAsync();
            return Map(entity);
        }

        public async Task<ProductSubCategoryReadDto?> UpdateAsync(UpdateProductSubCategoryDto dto)
        {
            var entity = await _context.ProductSubCategories.FindAsync(dto.Id);
            if (entity == null) return null;
            entity.CategoryId = dto.CategoryId; entity.Name = dto.Name;
            entity.Description = dto.Description; entity.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();
            return Map(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.ProductSubCategories.FindAsync(id);
            if (entity == null) return false;
            _context.ProductSubCategories.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
