using AppIt.Core.AppServices;
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
            await EnsureUniqueNameAsync(dto.Name, null);
            await CatalogConstraints.ValidateCategoryAsync(_context, dto.ProductCategoryId);
            var categoryName = await CatalogConstraints.ResolveCategoryNameAsync(_context, dto.ProductCategoryId);

            var product = new Product
            {
                Name = dto.Name.Trim(),
                Category = categoryName,
                ProductCategoryId = dto.ProductCategoryId,
                MaxPax = dto.MaxPax,
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

            await EnsureUniqueNameAsync(dto.Name, dto.ProductId);
            await CatalogConstraints.ValidateCategoryAsync(_context, dto.ProductCategoryId);
            var categoryName = await CatalogConstraints.ResolveCategoryNameAsync(_context, dto.ProductCategoryId);

            product.Name = dto.Name.Trim();
            product.Category = categoryName;
            product.ProductCategoryId = dto.ProductCategoryId;
            product.MaxPax = dto.MaxPax;
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
            var categoryNames = await CategoryNamesForAsync(products.Select(p => p.ProductCategoryId));
            return products.Select(p => ToReadDto(p, prices, categoryNames));
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
            var categoryNames = await CategoryNamesForAsync(new[] { product.ProductCategoryId });
            return ToReadDto(product, prices, categoryNames);
        }

        private static ProductReadDto ToReadDto(
            Product product,
            ILookup<int, ServicePriceReadDto> prices,
            IReadOnlyDictionary<int, string> categoryNames) => new()
        {
            ProductId = product.ProductId,
            Name = product.Name,
            Category = product.Category,
            ProductCategoryId = product.ProductCategoryId,
            CategoryName = product.ProductCategoryId.HasValue && categoryNames.TryGetValue(product.ProductCategoryId.Value, out var name)
                ? name
                : product.Category,
            MaxPax = product.MaxPax,
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

        private async Task<IReadOnlyDictionary<int, string>> CategoryNamesForAsync(IEnumerable<int?> categoryIds)
        {
            var ids = categoryIds.Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();
            if (ids.Count == 0) return new Dictionary<int, string>();

            return await _context.ProductCategories
                .AsNoTracking()
                .Where(c => ids.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name);
        }

        private async Task EnsureUniqueNameAsync(string name, int? currentId)
        {
            var normalized = (name ?? string.Empty).Trim().ToLower();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                throw new InvalidOperationException("Product name is required.");
            }

            var exists = await _context.Products.AnyAsync(p =>
                p.ProductId != currentId
                && (p.Name ?? string.Empty).ToLower() == normalized);
            if (exists)
            {
                throw new InvalidOperationException($"A product named '{name.Trim()}' already exists.");
            }
        }
    }
}
