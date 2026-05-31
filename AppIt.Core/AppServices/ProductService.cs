using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Core.Services
{
    public class ProductService : IProductService
    {
        private readonly AppItDbContext _context;

        public ProductService(AppItDbContext context)
        {
            _context = context;
        }

        public async Task<ProductReadDto> CreateAsync(CreateProductDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Category = NormalizeCategory(dto.Category),
                Description = dto.Description,
                BasePriceUsd = dto.BasePriceUsd,
                IsActive = dto.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            await EnsureUsdPriceAsync(product.ProductId, product.BasePriceUsd);
            return await ToReadDtoAsync(product);
        }

        public async Task<ProductReadDto?> UpdateAsync(UpdateProductDto dto)
        {
            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null) return null;

            product.Name = dto.Name;
            product.Category = NormalizeCategory(dto.Category);
            product.Description = dto.Description;
            product.BasePriceUsd = dto.BasePriceUsd;
            product.IsActive = dto.IsActive;
            

            await _context.SaveChangesAsync();

            await EnsureUsdPriceAsync(product.ProductId, product.BasePriceUsd);
            return await ToReadDtoAsync(product);
        }

        public async Task<bool> DeleteAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ProductReadDto?> GetByIdAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return null;

            return await ToReadDtoAsync(product);
        }

        public async Task<IEnumerable<ProductReadDto>> GetAllAsync()
        {
            var products = await _context.Products.AsNoTracking().ToListAsync();
            var prices = await PricesForAsync(products.Select(p => p.ProductId));
            return products.Select(p => ToReadDto(p, prices));
        }

        private async Task EnsureUsdPriceAsync(int productId, decimal basePriceUsd)
        {
            var price = await _context.ServicePrices
                .FirstOrDefaultAsync(p => p.ServiceType == "Product" && p.ServiceId == productId && p.CurrencyCode == "USD" && p.IsActive);
            if (price == null)
            {
                _context.ServicePrices.Add(new ServicePrice
                {
                    ServiceType = "Product",
                    ServiceId = productId,
                    CurrencyCode = "USD",
                    UnitPrice = basePriceUsd,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                price.UnitPrice = basePriceUsd;
            }

            await _context.SaveChangesAsync();
        }

        private async Task<ProductReadDto> ToReadDtoAsync(Product product)
        {
            var prices = await PricesForAsync(new[] { product.ProductId });
            return ToReadDto(product, prices);
        }

        private static ProductReadDto ToReadDto(Product product, ILookup<int, ServicePriceReadDto> prices) => new()
        {
            ProductId = product.ProductId,
            Name = product.Name,
            Category = NormalizeCategory(product.Category),
            Description = product.Description,
            BasePriceUsd = product.BasePriceUsd,
            IsActive = product.IsActive,
            CreatedDate = product.CreatedDate,
            Prices = prices[product.ProductId].ToList()
        };

        private async Task<ILookup<int, ServicePriceReadDto>> PricesForAsync(IEnumerable<int> productIds)
        {
            var ids = productIds.ToList();
            var prices = await _context.ServicePrices
                .AsNoTracking()
                .Where(p => p.ServiceType == "Product" && ids.Contains(p.ServiceId) && p.IsActive)
                .OrderBy(p => p.CurrencyCode)
                .Select(p => new ServicePriceReadDto
                {
                    Id = p.Id,
                    ServiceType = p.ServiceType,
                    ServiceId = p.ServiceId,
                    CurrencyCode = p.CurrencyCode,
                    UnitPrice = p.UnitPrice,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();
            return prices.ToLookup(p => p.ServiceId);
        }

        private static string NormalizeCategory(string? category)
        {
            return string.IsNullOrWhiteSpace(category) ? "Product" : "Product";
        }
    }
}
