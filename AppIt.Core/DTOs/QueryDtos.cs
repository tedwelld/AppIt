namespace AppIt.Core.DTOs
{
    public class ListQueryOptions
    {
        private const int MaxSize = 200;
        private int _page = 1;
        private const int DefaultPageSize = 10;
        private int _pageSize = DefaultPageSize;

        public int Page
        {
            get => _page;
            set => _page = value < 1 ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? DefaultPageSize : Math.Min(value, MaxSize);
        }

        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public string SortDirection { get; set; } = "asc";
        public string? Date { get; set; }
        public string? Month { get; set; }
        public string? Year { get; set; }
        public string? DateFrom { get; set; }
        public string? DateTo { get; set; }
    }

    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
