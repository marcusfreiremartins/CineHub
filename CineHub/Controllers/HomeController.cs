using Microsoft.AspNetCore.Mvc;
using CineHub.Services;
using CineHub.Models.ViewModels;
using CineHub.Models;
using CineHub.Configuration;
using Microsoft.Extensions.Options;

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

        public async Task<IActionResult> Index()
        {
            try
            {
                var movies = await _movieService.GetPopularMoviesAsync();
                var viewModel = new MovieIndexViewModel
                {
                    Movies = movies,
                    ImageBaseUrl = _imageSettings.BaseUrl
                };

                // Mensagem de boas-vindas para novos usuários (opcional)
                if (IsUserLoggedIn() && TempData["IsNewUser"] != null)
                {
                    var userName = HttpContext.Session.GetString("UserName");
                    ShowSuccess($"Bem-vindo ao CineHub, {userName}! Explore nossos filmes populares! 🎬");
                }

                return View(viewModel);
            }
            catch (Exception)
            {
                ShowError("Erro ao carregar filmes populares. Tente novamente mais tarde.");
                return View(new MovieIndexViewModel { Movies = new List<Movie>(), ImageBaseUrl = _imageSettings.BaseUrl });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Search(string q = "", int page = 1)
        {
            try
            {
                List<Movie> movies;

                if (string.IsNullOrWhiteSpace(q))
                {
                    movies = await _movieService.GetPopularMoviesAsync(page);
                }
                else
                {
                    movies = await _movieService.SearchMoviesAsync(q, page);

                    if (!movies.Any())
                    {
                        ShowInfo($"Nenhum filme encontrado para '{q}'. Tente outros termos de busca. 🔍");
                    }
                }

                var viewModel = new MovieIndexViewModel
                {
                    Movies = movies,
                    Search = q,
                    CurrentPage = page,
                    ImageBaseUrl = _imageSettings.BaseUrl
                };

                return View("Index", viewModel);
            }
            catch (Exception)
            {
                ShowError("Erro na busca. Tente novamente.");
                return View("Index", new MovieIndexViewModel { Movies = new List<Movie>(), ImageBaseUrl = _imageSettings.BaseUrl });
            }
        }
    }
}
