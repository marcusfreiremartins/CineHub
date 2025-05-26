using Microsoft.AspNetCore.Mvc;
using CineHub.Services;
using CineHub.Models.ViewModels;
using CineHub.Models;
using CineHub.Configuration;
using Microsoft.Extensions.Options;

namespace CineHub.Controllers
{
    public class MoviesController : BaseController
    {
        private readonly MovieService _movieService;
        private readonly ImageSettings _imageSettings;

        public MoviesController(MovieService movieService, IOptions<ImageSettings> imageSettings)
        {
            _movieService = movieService;
            _imageSettings = imageSettings.Value;
        }

        public async Task<IActionResult> Index(string search = "", int page = 1)
        {
            try
            {
                List<Movie> movies;

                if (string.IsNullOrWhiteSpace(search))
                {
                    movies = await _movieService.GetPopularMoviesAsync(page);
                }
                else
                {
                    movies = await _movieService.SearchMoviesAsync(search, page);

                    if (!movies.Any())
                    {
                        ShowInfo($"Nenhum filme encontrado para '{search}'. Experimente outros termos! 🎭");
                    }
                }

                var viewModel = new MovieIndexViewModel
                {
                    Movies = movies,
                    Search = search,
                    CurrentPage = page,
                    ImageBaseUrl = _imageSettings.BaseUrl
                };

                return View(viewModel);
            }
            catch (Exception)
            {
                ShowError("Erro ao carregar filmes. Tente novamente mais tarde.");
                return View(new MovieIndexViewModel { Movies = new List<Movie>(), ImageBaseUrl = _imageSettings.BaseUrl });
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var movie = await _movieService.GetMovieByIdAsync(id);
                if (movie == null)
                {
                    ShowError("Filme não encontrado!");
                    return RedirectToAction("Index");
                }

                var viewModel = new MovieDetailsViewModel
                {
                    Movie = movie,
                    ImageBaseUrl = _imageSettings.BaseUrl
                };

                return View(viewModel);
            }
            catch (Exception)
            {
                ShowError("Erro ao carregar detalhes do filme.");
                return RedirectToAction("Index");
            }
        }
    }
}