using System.Reflection;
using AppIt.Core.DTOs;

namespace AppIt.Api.Infrastructure
{
    public static class ListQueryExtensions
    {
        public static PagedResult<T> ApplyQuery<T>(
            this IEnumerable<T> source,
            ListQueryOptions? query,
            params string[] searchFields)
        {
            var options = query ?? new ListQueryOptions();
            var working = source ?? Enumerable.Empty<T>();

            if (!string.IsNullOrWhiteSpace(options.Search))
            {
                var term = options.Search.Trim();
                working = working.Where(item => MatchesSearch(item, term, searchFields));
            }

            var sortBy = string.IsNullOrWhiteSpace(options.SortBy) ? null : options.SortBy.Trim();
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var desc = string.Equals(options.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
                working = SortByProperty(working, sortBy!, desc);
            }

            var total = working.Count();
            var page = options.Page < 1 ? 1 : options.Page;
            var pageSize = options.PageSize < 1 ? 20 : options.PageSize;
            var skip = (page - 1) * pageSize;
            var items = working.Skip(skip).Take(pageSize).ToList();

            return new PagedResult<T>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        private static bool MatchesSearch<T>(T item, string term, string[] searchFields)
        {
            if (item == null)
            {
                return false;
            }

            var props = GetSearchableProperties(typeof(T), searchFields);
            foreach (var prop in props)
            {
                var value = prop.GetValue(item)?.ToString();
                if (!string.IsNullOrWhiteSpace(value)
                    && value.Contains(term, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static IEnumerable<PropertyInfo> GetSearchableProperties(Type type, string[] searchFields)
        {
            var all = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead)
                .ToList();

            if (searchFields.Length == 0)
            {
                return all.Where(p =>
                    p.PropertyType == typeof(string)
                    || p.PropertyType.IsValueType
                    || Nullable.GetUnderlyingType(p.PropertyType)?.IsValueType == true);
            }

            return all.Where(p => searchFields.Any(f => string.Equals(f, p.Name, StringComparison.OrdinalIgnoreCase)));
        }

        private static IEnumerable<T> SortByProperty<T>(IEnumerable<T> source, string propertyName, bool descending)
        {
            var prop = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase) && p.CanRead);

            if (prop == null)
            {
                return source;
            }

            return descending
                ? source.OrderByDescending(x => prop.GetValue(x))
                : source.OrderBy(x => prop.GetValue(x));
        }
    }
}
