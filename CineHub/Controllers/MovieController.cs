using Microsoft.AspNetCore.Mvc;
using CineHub.Services;
using CineHub.Models.ViewModels;
using CineHub.Models;
using CineHub.Configuration;
using Microsoft.Extensions.Options;

namespace CineHub.Controllers
{
    public class MoviesController : Controller
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
            List<Movie> movies;

            if (string.IsNullOrWhiteSpace(search))
            {
                movies = await _movieService.GetPopularMoviesAsync(page);
            }
            else
            {
                movies = await _movieService.SearchMoviesAsync(search, page);
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

        public async Task<IActionResult> Details(int id)
        {
            var movie = await _movieService.GetMovieByIdAsync(id);
            if (movie == null)
            {
                return NotFound();
            }

            var viewModel = new MovieDetailsViewModel
            {
                Movie = movie,
                ImageBaseUrl = _imageSettings.BaseUrl
            };

            return View(viewModel);
        }
    }
}