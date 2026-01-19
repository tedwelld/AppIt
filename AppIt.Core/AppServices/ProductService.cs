using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AppIt.Core.AppServices
{
    public class ProductService : IProductService
    {
        public Task<ServiceResponse<ProductDto>> CreateProductAsync(CreateProductDto createDto)
        {
            return Task.FromResult(new ServiceResponse<ProductDto>(new ProductDto(), "Product created (stub)"));
        }

        public Task<ServiceResponse<ProductResponseDto>> GetProductsAsync(ProductFilterDto filterDto)
        {
            return Task.FromResult(new ServiceResponse<ProductResponseDto>(new ProductResponseDto(), "Products retrieved (stub)"));
        }

        public Task<ServiceResponse<ProductDto>> GetProductByIdAsync(int id)
        {
            return Task.FromResult(new ServiceResponse<ProductDto>(new ProductDto(), "Product retrieved (stub)"));
        }

        public Task<ServiceResponse<bool>> DeleteProductAsync(int id)
        {
            return Task.FromResult(new ServiceResponse<bool>(true, "Product deleted (stub)"));
        }

        public Task<ServiceResponse<ProductDto>> UpdateProductAsync(int id, UpdateProductDto updateDto)
        {
            return Task.FromResult(new ServiceResponse<ProductDto>(new ProductDto(), "Product updated (stub)"));
        }
    }
}
