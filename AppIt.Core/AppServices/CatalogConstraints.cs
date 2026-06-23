using AppIt.Data;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Core.AppServices
{
    internal static class CatalogConstraints
    {
        public static async Task ValidateCategoryAsync(AppItDbContext context, int? categoryId)
        {
            if (!categoryId.HasValue) return;

            var exists = await context.ProductCategories
                .AsNoTracking()
                .AnyAsync(c => c.Id == categoryId.Value && c.IsActive);
            if (!exists)
            {
                throw new InvalidOperationException("Selected product category does not exist or is inactive.");
            }
        }

        public static async Task<string> ResolveCategoryNameAsync(AppItDbContext context, int? categoryId)
        {
            if (!categoryId.HasValue) return "Uncategorized";

            var name = await context.ProductCategories
                .AsNoTracking()
                .Where(c => c.Id == categoryId.Value)
                .Select(c => c.Name)
                .FirstOrDefaultAsync();

            return string.IsNullOrWhiteSpace(name) ? "Uncategorized" : name.Trim();
        }
    }
}
