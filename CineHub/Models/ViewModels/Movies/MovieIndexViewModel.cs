using CineHub.Models.ViewModels.Base;

namespace CineHub.Models.ViewModels.Movies
{
    public class MovieIndexViewModel : BaseViewModel
    {
        public List<Movie> PopularMovies { get; set; } = new();
        public List<Movie> TopRatedMovies { get; set; } = new();
        public List<Movie> Movies { get; set; } = new();
        public string Search { get; set; } = string.Empty;

        // Method to facilitate the creation of the ViewModel from the PaginatedResult
        public static MovieIndexViewModel FromPaginatedResult(PaginatedResult<Movie> paginatedResult, string search = "")
        {
            var viewModel = new MovieIndexViewModel
            {
                Movies = paginatedResult.Items,
                Search = search
            };

            viewModel.SetPaginationProperties(paginatedResult);
            return viewModel;
        }
    }
}