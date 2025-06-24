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
                TotalItems = 0,
                HasSearched = false,
                HasPreviousPage = false,
                HasNextPage = false,
                PreviousPage = null,
                NextPage = null,
                PageNumbers = new List<int>()
            };

            return View(viewModel);
        }

        // Handles search requests with pagination
        [HttpGet]
        public async Task<IActionResult> Results(string query = "", int? minRating = null, int? releaseYear = null, int page = 1)
        {
            try
            {
                // Ensure page is at least 1
                page = Math.Max(1, page);

                bool hasSearched = !string.IsNullOrWhiteSpace(query) || minRating.HasValue || releaseYear.HasValue;
                PaginatedResult<Movie> paginatedResult;

                if (hasSearched)
                {
                    // Use the AdvancedSearchAsync method that returns PaginatedResult
                    paginatedResult = await _movieService.AdvancedSearchAsync(
                        query: !string.IsNullOrWhiteSpace(query) ? query : null,
                        minRating: minRating,
                        releaseYear: releaseYear,
                        page: page
                    );

                    // Check if no results found
                    if (!paginatedResult.Items.Any() && hasSearched)
                    {
                        string filterMessage = BuildFilterMessage(query, minRating, releaseYear);
                        TempData["Info"] = $"Nenhum filme encontrado {filterMessage}. Experimente outros termos! 🎭";
                    }
                    else if (paginatedResult.Items.Any())
                    {
                        // Show results summary
                        string filterMessage = BuildFilterMessage(query, minRating, releaseYear);
                        string resultsMessage = $"Encontrados {paginatedResult.TotalItems} filme(s) {filterMessage}";
                        TempData["Success"] = resultsMessage;
                    }
                }
                else
                {
                    // No search criteria provided, return empty result
                    paginatedResult = new PaginatedResult<Movie>(new List<Movie>(), 0, page, 20);
                }

                // Create view model using the static method
                var viewModel = SearchIndexViewModel.FromPaginatedResult(
                    paginatedResult,
                    query,
                    minRating,
                    releaseYear,
                    hasSearched
                );

                // Set the image base URL
                viewModel.ImageBaseUrl = _imageSettings.BaseUrl;

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na busca: {ex.Message}");

                TempData["Error"] = "Erro ao realizar a pesquisa. Tente novamente mais tarde.";
                bool hasSearchedForError = !string.IsNullOrWhiteSpace(query) || minRating.HasValue || releaseYear.HasValue;

                // Return empty result in case of error
                var emptyResult = new PaginatedResult<Movie>(new List<Movie>(), 0, page, 20);
                var errorViewModel = SearchIndexViewModel.FromPaginatedResult(
                    emptyResult,
                    query ?? string.Empty,
                    minRating,
                    releaseYear,
                    hasSearchedForError
                );

                errorViewModel.ImageBaseUrl = _imageSettings.BaseUrl;

                return View("Index", errorViewModel);
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

        // Action for pagination
        [HttpGet]
        public async Task<IActionResult> LoadPage(string query = "", int? minRating = null, int? releaseYear = null, int page = 1)
        {
            try
            {
                page = Math.Max(1, page);
                bool hasSearched = !string.IsNullOrWhiteSpace(query) || minRating.HasValue || releaseYear.HasValue;

                if (!hasSearched)
                {
                    return Json(new { success = false, message = "Nenhum critério de busca fornecido." });
                }

                var paginatedResult = await _movieService.AdvancedSearchAsync(
                    query: !string.IsNullOrWhiteSpace(query) ? query : null,
                    minRating: minRating,
                    releaseYear: releaseYear,
                    page: page
                );

                var viewModel = SearchIndexViewModel.FromPaginatedResult(
                    paginatedResult,
                    query,
                    minRating,
                    releaseYear,
                    hasSearched
                );

                viewModel.ImageBaseUrl = _imageSettings.BaseUrl;

                return PartialView("_SearchResults", viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar página: {ex.Message}");
                return Json(new { success = false, message = "Erro ao carregar a página." });
            }
        }
    }
}