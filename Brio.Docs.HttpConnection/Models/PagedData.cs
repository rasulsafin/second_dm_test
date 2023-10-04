namespace Brio.Docs.HttpConnection.Models
{
    public class PagedData : System.IEquatable<PagedData>
    {
        public PagedData(int currentPage, int pageSize, int totalPages, int totalCount)
        {
            CurrentPage = currentPage;
            TotalPages = totalPages;
            PageSize = pageSize;
            TotalCount = totalCount;
        }

        public int CurrentPage { get; }

        public int TotalPages { get; }

        public int PageSize { get; }

        public int TotalCount { get; }

        public bool HasPrevious => CurrentPage > 1;

        public bool HasNext => CurrentPage < TotalPages;

        public bool Equals(PagedData other)
        {
            return this.CurrentPage == other.CurrentPage
                && this.TotalPages == other.TotalPages
                && this.PageSize == other.PageSize
                && this.TotalCount == other.TotalCount;
        }
    }
}
