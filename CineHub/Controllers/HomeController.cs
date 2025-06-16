using Microsoft.AspNetCore.Mvc;
using CineHub.Services;
using CineHub.Models;
using CineHub.Configuration;
using Microsoft.Extensions.Options;
using CineHub.Models.ViewModels.Movies;

namespace CineHub.Controllers
{
    public class HomeController : BaseController
    {
        private readonly MovieService _movieService;
        private readonly ImageSettings _imageSettings;

        public HomeController(MovieService movieService, IOptions<ImageSettings> imageSettings)
        {
            _movieService = movieService;
            _imageSettings = imageSettings.Value;
        }

        // Displays the home page with popular movies, now playing, and top rated
        public async Task<IActionResult> Index()
        {
            var viewModel = new MovieIndexViewModel
            {
                ImageBaseUrl = _imageSettings.BaseUrl,
                PopularMovies = new List<Movie>(),
                TopRatedMovies = new List<Movie>()
            };

            try
            {
                var popularMovies = await _movieService.GetPopularMoviesAsync();
                viewModel.PopularMovies = popularMovies.Take(12).ToList();

                var topRatedMovies = await _movieService.GetTopRatedMoviesAsync();
                viewModel.TopRatedMovies = topRatedMovies.Take(12).ToList();

                if (IsUserLoggedIn() && TempData["IsNewUser"] != null)
                {
                    var userName = HttpContext.Session.GetString("UserName");
                    ShowSuccess($"Bem-vindo ao CineHub, {userName}! Explore nossos filmes populares! 🎬");
                    TempData.Remove("IsNewUser");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar filmes: {ex.Message}");
                ShowError("Erro ao carregar filmes. Tente novamente mais tarde.");
            }

            return View(viewModel);
        }

        // Handles movie search and displays results
        [HttpGet]
        public async Task<IActionResult> Search(string q)
        {
            var viewModel = new MovieIndexViewModel
            {
                ImageBaseUrl = _imageSettings.BaseUrl,
                Search = q ?? string.Empty,
                Movies = new List<Movie>(),
                PopularMovies = new List<Movie>(),
                TopRatedMovies = new List<Movie>()
            };

            if (!string.IsNullOrWhiteSpace(q))
            {
                try
                {
                    var searchResults = await _movieService.SearchMoviesAsync(q);
                    viewModel.Movies = searchResults;

                    if (!searchResults.Any())
                    {
                        ShowInfo($"Nenhum filme encontrado para '{q}'. Tente outros termos de busca. 🔍");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro na busca: {ex.Message}");
                    ShowError("Erro na busca. Tente novamente.");
                    viewModel.Movies = new List<Movie>();
                }
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }

            return View("Index", viewModel);
        }
    }
}