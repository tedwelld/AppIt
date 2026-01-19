using System;
using System.Collections.Generic;
using System.Text;
using AppIt.Core.DTOs;

namespace AppIt.Core.Interfaces
{
    public interface IProductService
    {
        Task<ServiceResponse<ProductResponseDto>> GetProductsAsync(ProductFilterDto filterDto);
        Task<ServiceResponse<ProductDto>> CreateProductAsync(CreateProductDto createDto);
        Task<ServiceResponse<ProductDto>> UpdateProductAsync(int id, UpdateProductDto updateDto);
        Task<ServiceResponse<bool>> DeleteProductAsync(int id);
        Task<ServiceResponse<ProductDto>> GetProductByIdAsync(int id);
    }
}
