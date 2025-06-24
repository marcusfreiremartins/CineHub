namespace CineHub.Models.ViewModels.Base
{
    public abstract class BaseViewModel
    {
        public string ImageBaseUrl { get; set; } = string.Empty;

        // Pagination properties
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalItems { get; set; } = 0;
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
        public int? PreviousPage { get; set; }
        public int? NextPage { get; set; }
        public List<int> PageNumbers { get; set; } = new();

        // Common method for configuring pagination properties
        protected void SetPaginationProperties(PaginatedResult<Movie> paginatedResult)
        {
            CurrentPage = paginatedResult.CurrentPage;
            TotalPages = paginatedResult.TotalPages;
            TotalItems = paginatedResult.TotalItems;
            HasPreviousPage = paginatedResult.HasPreviousPage;
            HasNextPage = paginatedResult.HasNextPage;
            PreviousPage = paginatedResult.PreviousPage;
            NextPage = paginatedResult.NextPage;
            PageNumbers = paginatedResult.GetPageNumbers();
        }
    }
}