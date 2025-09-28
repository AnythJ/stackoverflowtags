namespace sotagapi.Models
{
    public class PagedResponse<T>
    {
        public int TotalCount { get; set; }
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    }

    public enum TagSortBy { Share, Count, Name, FetchedAt }
    public enum SortOrder { Asc, Desc }
}
