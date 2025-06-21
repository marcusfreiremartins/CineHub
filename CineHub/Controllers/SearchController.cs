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
                    movies = await _movieService.AdvancedSearchAsync(
                        query: !string.IsNullOrWhiteSpace(query) ? query : null,
                        minRating: minRating,
                        releaseYear: releaseYear,
                        page: page
                    );

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
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na busca: {ex.Message}");

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