using Microsoft.AspNetCore.Mvc;
using CineHub.Services;
using CineHub.Models.ViewModels;
using CineHub.Models;
using CineHub.Configuration;
using Microsoft.Extensions.Options;

namespace CineHub.Controllers
{
    public class HomeController : Controller
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
            var movies = await _movieService.GetPopularMoviesAsync();
            var viewModel = new MovieIndexViewModel
            {
                Movies = movies,
                ImageBaseUrl = _imageSettings.BaseUrl
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string q = "", int page = 1)
        {
            List<Movie> movies;

            if (string.IsNullOrWhiteSpace(q))
            {
                movies = await _movieService.GetPopularMoviesAsync(page);
            }
            else
            {
                movies = await _movieService.SearchMoviesAsync(q, page);
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
    }
}