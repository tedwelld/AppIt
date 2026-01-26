using System;
using System.Collections.Generic;
using System.Text;
using AppIt.Core.DTOs;
using System.Collections.Generic;
 using System.Threading.Tasks;

    namespace AppIt.Core.Interfaces.Services
    {
        public interface IProductService
        {
            Task<ProductReadDto> CreateAsync(CreateProductDto dto);
            Task<ProductReadDto?> UpdateAsync(UpdateProductDto dto);
            Task<bool> DeleteAsync(int productId);
            Task<ProductReadDto?> GetByIdAsync(int productId);
            Task<IEnumerable<ProductReadDto>> GetAllAsync();
        }
    }


