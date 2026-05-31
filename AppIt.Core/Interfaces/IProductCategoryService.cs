using AppIt.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppIt.Core.Interfaces.Services
{
    public interface IProductCategoryService
    {
        Task<IEnumerable<ProductCategoryReadDto>> GetAllAsync();
        Task<ProductCategoryReadDto?> GetByIdAsync(int id);
        Task<ProductCategoryReadDto> CreateAsync(CreateProductCategoryDto dto);
        Task<ProductCategoryReadDto?> UpdateAsync(UpdateProductCategoryDto dto);
        Task<bool> DeleteAsync(int id);
    }

    public interface IProductSubCategoryService
    {
        Task<IEnumerable<ProductSubCategoryReadDto>> GetAllAsync();
        Task<ProductSubCategoryReadDto?> GetByIdAsync(int id);
        Task<ProductSubCategoryReadDto> CreateAsync(CreateProductSubCategoryDto dto);
        Task<ProductSubCategoryReadDto?> UpdateAsync(UpdateProductSubCategoryDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
