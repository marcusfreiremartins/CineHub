using Microsoft.AspNetCore.Mvc;
using CineHub.Services;
using CineHub.Models;
using CineHub.Configuration;
using Microsoft.Extensions.Options;
using CineHub.Models.ViewModels.Search;

namespace CineHub.Controllers
{
    public class SearchController : BaseController
    {
        private readonly MovieService _movieService;
        private readonly ImageSettings _imageSettings;

        public SearchController(
            MovieService movieService,
            IOptions<ImageSettings> imageSettings)
        {
            _movieService = movieService;
            _imageSettings = imageSettings.Value;
        }

        // Displays the search page
        [HttpGet]
        public IActionResult Index()
        {
            var viewModel = new SearchIndexViewModel
            {
                ImageBaseUrl = _imageSettings.BaseUrl,
                Movies = new List<Movie>(),
                SearchQuery = string.Empty,
                MinRating = null,
                ReleaseYear = null,
                CurrentPage = 1,
                TotalPages = 1,
                HasSearched = false
            };

            return View(viewModel);
        }

        // Handles search requests
        [HttpGet]
        public async Task<IActionResult> Results(string query = "", int? minRating = null, int? releaseYear = null, int page = 1)
        {
            try
            {
                List<Movie> movies = new List<Movie>();
                bool hasSearched = !string.IsNullOrWhiteSpace(query) || minRating.HasValue || releaseYear.HasValue;

                if (hasSearched)
                {
                    // Search by query if provided
                    if (!string.IsNullOrWhiteSpace(query))
                    {
                        movies = await _movieService.SearchMoviesAsync(query, page);
                    }
                    else
                    {
                        // If no query but has filters, get popular movies to filter
                        movies = await _movieService.GetPopularMoviesAsync(page);
                    }

                    // Apply filters
                    movies = ApplyFilters(movies, minRating, releaseYear);

                    if (!movies.Any() && hasSearched)
                    {
                        string filterMessage = BuildFilterMessage(query, minRating, releaseYear);
                        TempData["Info"] = $"Nenhum filme encontrado {filterMessage}. Experimente outros termos! 🎭";
                    }
                }

                var viewModel = new SearchIndexViewModel
                {
                    ImageBaseUrl = _imageSettings.BaseUrl,
                    Movies = movies,
                    SearchQuery = query ?? string.Empty,
                    MinRating = minRating,
                    ReleaseYear = releaseYear,
                    CurrentPage = page,
                    TotalPages = 1,
                    HasSearched = hasSearched
                };

                return View("Index", viewModel);
            }
            catch (Exception)
            {
                TempData["Error"] = "Erro ao realizar a pesquisa. Tente novamente mais tarde.";
                bool hasSearchedForError = !string.IsNullOrWhiteSpace(query) || minRating.HasValue || releaseYear.HasValue;
                return View("Index", new SearchIndexViewModel
                {
                    ImageBaseUrl = _imageSettings.BaseUrl,
                    Movies = new List<Movie>(),
                    SearchQuery = query ?? string.Empty,
                    MinRating = minRating,
                    ReleaseYear = releaseYear,
                    HasSearched = hasSearchedForError
                });
            }
        }

        // Apply filters to the movie list
        private List<Movie> ApplyFilters(List<Movie> movies, int? minRating, int? releaseYear)
        {
            var filteredMovies = movies.AsQueryable();

            // Filter by minimum rating
            if (minRating.HasValue)
            {
                filteredMovies = filteredMovies.Where(m => (int)Math.Round(m.VoteAverage) >= minRating.Value);
            }

            // Filter by release year
            if (releaseYear.HasValue)
            {
                filteredMovies = filteredMovies.Where(m => m.ReleaseDate.HasValue && m.ReleaseDate.Value.Year == releaseYear.Value);
            }

            return filteredMovies.ToList();
        }

        // Build filter message for user feedback
        private string BuildFilterMessage(string? query, int? minRating, int? releaseYear)
        {
            var filters = new List<string>();

            if (!string.IsNullOrWhiteSpace(query))
                filters.Add($"com o termo '{query}'");

            if (minRating.HasValue)
                filters.Add($"com nota mínima {minRating}");

            if (releaseYear.HasValue)
                filters.Add($"do ano {releaseYear}");

            return filters.Any() ? string.Join(" ", filters) : "";
        }
    }
}