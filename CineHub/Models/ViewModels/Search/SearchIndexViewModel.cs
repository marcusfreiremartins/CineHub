using CineHub.Models.ViewModels.Base;

namespace CineHub.Models.ViewModels.Search
{
    public class SearchIndexViewModel : BaseViewModel
    {
        public List<Movie> Movies { get; set; } = new List<Movie>();
        public string SearchQuery { get; set; } = string.Empty;
        public int? MinRating { get; set; }
        public int? ReleaseYear { get; set; }
        public bool HasSearched { get; set; } = false;

        // Method to facilitate the creation of the ViewModel from the PaginatedResult
        public static SearchIndexViewModel FromPaginatedResult(PaginatedResult<Movie> paginatedResult, string query = "", int? minRating = null, int? releaseYear = null, bool hasSearched = false)
        {
            var viewModel = new SearchIndexViewModel
            {
                Movies = paginatedResult.Items,
                SearchQuery = query,
                MinRating = minRating,
                ReleaseYear = releaseYear,
                HasSearched = hasSearched
            };

            viewModel.SetPaginationProperties(paginatedResult);
            return viewModel;
        }
    }
}