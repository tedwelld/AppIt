using System.Reflection;
using AppIt.Core.DTOs;
using System.Globalization;

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

            if (HasDateFilters(options))
            {
                working = working.Where(item => MatchesDateFilters(item, options, searchFields));
            }

            var sortBy = string.IsNullOrWhiteSpace(options.SortBy) ? null : options.SortBy.Trim();
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var desc = string.Equals(options.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
                working = SortByProperty(working, sortBy!, desc);
            }

            var total = working.Count();
            var page = options.Page < 1 ? 1 : options.Page;
            var pageSize = options.PageSize < 1 ? 10 : options.PageSize;
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

        private static bool HasDateFilters(ListQueryOptions options)
        {
            return !string.IsNullOrWhiteSpace(options.Date)
                || !string.IsNullOrWhiteSpace(options.Month)
                || !string.IsNullOrWhiteSpace(options.Year)
                || !string.IsNullOrWhiteSpace(options.DateFrom)
                || !string.IsNullOrWhiteSpace(options.DateTo);
        }

        private static bool MatchesDateFilters<T>(T item, ListQueryOptions options, string[] searchFields)
        {
            if (item == null)
            {
                return false;
            }

            var props = GetDateProperties(typeof(T), searchFields);
            if (props.Count == 0)
            {
                return false;
            }

            var dateFilter = ParseDateFilter(options.Date);
            var monthFilter = ParseMonthFilter(options.Month);
            var yearFilter = ParseYearFilter(options.Year);
            var dateFromFilter = ParseDateFilter(options.DateFrom);
            var dateToFilter = ParseDateFilter(options.DateTo);

            foreach (var prop in props)
            {
                if (!TryGetDate(prop.GetValue(item), out var value))
                {
                    continue;
                }

                var date = value.Date;
                if (dateFilter.HasValue && date != dateFilter.Value.Date)
                {
                    continue;
                }

                if (monthFilter.HasValue
                    && (date.Year != monthFilter.Value.year || date.Month != monthFilter.Value.month))
                {
                    continue;
                }

                if (yearFilter.HasValue && date.Year != yearFilter.Value)
                {
                    continue;
                }

                if (dateFromFilter.HasValue && date < dateFromFilter.Value.Date)
                {
                    continue;
                }

                if (dateToFilter.HasValue && date > dateToFilter.Value.Date)
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        private static List<PropertyInfo> GetDateProperties(Type type, string[] searchFields)
        {
            var all = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead)
                .ToList();

            var candidates = searchFields.Length == 0
                ? all
                : all.Where(p => searchFields.Any(f => string.Equals(f, p.Name, StringComparison.OrdinalIgnoreCase))).ToList();

            return candidates
                .Where(p => IsDateType(p.PropertyType))
                .ToList();
        }

        private static bool IsDateType(Type type)
        {
            var underlying = Nullable.GetUnderlyingType(type) ?? type;
            return underlying == typeof(DateTime)
                || underlying == typeof(DateTimeOffset)
                || underlying == typeof(string);
        }

        private static DateTime? ParseDateFilter(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return DateTime.TryParseExact(
                value.Trim(),
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var parsed)
                ? parsed
                : null;
        }

        private static (int year, int month)? ParseMonthFilter(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (!DateTime.TryParseExact(
                    value.Trim(),
                    "yyyy-MM",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var parsed))
            {
                return null;
            }

            return (parsed.Year, parsed.Month);
        }

        private static int? ParseYearFilter(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : null;
        }

        private static bool TryGetDate(object? value, out DateTime date)
        {
            switch (value)
            {
                case DateTime dt:
                    date = dt;
                    return true;
                case DateTimeOffset dto:
                    date = dto.UtcDateTime;
                    return true;
                case string text when !string.IsNullOrWhiteSpace(text):
                    if (DateTime.TryParse(
                        text.Trim(),
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out var parsed))
                    {
                        date = parsed;
                        return true;
                    }
                    break;
            }

            date = default;
            return false;
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
