namespace CineHub.Models
{
    public class PaginatedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public int? PreviousPage => HasPreviousPage ? CurrentPage - 1 : null;
        public int? NextPage => HasNextPage ? CurrentPage + 1 : null;

        public PaginatedResult(List<T> items, int totalCount, int currentPage, int pageSize)
        {
            Items = items;
            TotalItems = totalCount;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        }

        // Helper method to get the pages to be displayed in the navigation
        public List<int> GetPageNumbers(int maxPagesToShow = 5)
        {
            var pages = new List<int>();

            if (TotalPages <= maxPagesToShow)
            {
                for (int i = 1; i <= TotalPages; i++)
                {
                    pages.Add(i);
                }
            }
            else
            {
                int start = Math.Max(1, CurrentPage - maxPagesToShow / 2);
                int end = Math.Min(TotalPages, start + maxPagesToShow - 1);

                if (end - start < maxPagesToShow - 1)
                {
                    start = Math.Max(1, end - maxPagesToShow + 1);
                }

                for (int i = start; i <= end; i++)
                {
                    pages.Add(i);
                }
            }

            return pages;
        }
    }
}