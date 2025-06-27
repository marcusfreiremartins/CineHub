using Microsoft.AspNetCore.Mvc;
using CineHub.Services;
using CineHub.Models;
using CineHub.Configuration;
using Microsoft.Extensions.Options;
using CineHub.Models.ViewModels.Movies;

namespace CineHub.Controllers
{
    public class MoviesController : BaseController
    {
        private readonly MovieService _movieService;
        private readonly RatingService _ratingService;
        private readonly ImageSettings _imageSettings;

        public MoviesController(
            MovieService movieService,
            RatingService ratingService,
            IOptions<ImageSettings> imageSettings)
        {
            _movieService = movieService;
            _ratingService = ratingService;
            _imageSettings = imageSettings.Value;
        }

        // Displays a list of popular movies with pagination
        public async Task<IActionResult> Popular(string search = "", int page = 1)
        {
            if (page < 1) page = 1;

            try
            {
                PaginatedResult<Movie> paginatedResult;

                if (string.IsNullOrWhiteSpace(search))
                {
                    paginatedResult = await _movieService.GetPopularMoviesAsync(page);
                }
                else
                {
                    paginatedResult = await _movieService.SearchMoviesAsync(search, page);

                    if (!paginatedResult.Items.Any() && paginatedResult.CurrentPage == 1)
                    {
                        TempData["Info"] = $"Nenhum filme encontrado para '{search}'. Experimente outros termos! 🎭";
                    }
                }

                var viewModel = MovieIndexViewModel.FromPaginatedResult(paginatedResult, search);
                viewModel.ImageBaseUrl = _imageSettings.BaseUrl;

                return View(viewModel);
            }
            catch (Exception)
            {
                TempData["Error"] = "Erro ao carregar filmes. Tente novamente mais tarde.";

                var emptyResult = new PaginatedResult<Movie>(new List<Movie>(), 0, page, 20);
                var viewModel = MovieIndexViewModel.FromPaginatedResult(emptyResult, search);
                viewModel.ImageBaseUrl = _imageSettings.BaseUrl;

                return View(viewModel);
            }
        }

        // Displays a list of top rated movies with pagination
        public async Task<IActionResult> TopRated(string search = "", int page = 1)
        {
            if (page < 1) page = 1;

            try
            {
                PaginatedResult<Movie> paginatedResult;

                if (string.IsNullOrWhiteSpace(search))
                {
                    paginatedResult = await _movieService.GetTopRatedMoviesAsync(page);
                }
                else
                {
                    paginatedResult = await _movieService.SearchMoviesAsync(search, page);

                    if (!paginatedResult.Items.Any() && paginatedResult.CurrentPage == 1)
                    {
                        TempData["Info"] = $"Nenhum filme encontrado para '{search}'. Experimente outros termos! 🎭";
                    }
                }

                var viewModel = MovieIndexViewModel.FromPaginatedResult(paginatedResult, search);
                viewModel.ImageBaseUrl = _imageSettings.BaseUrl;

                return View(viewModel);
            }
            catch (Exception)
            {
                TempData["Error"] = "Erro ao carregar filmes mais bem avaliados. Tente novamente mais tarde.";

                var emptyResult = new PaginatedResult<Movie>(new List<Movie>(), 0, page, 20);
                var viewModel = MovieIndexViewModel.FromPaginatedResult(emptyResult, search);
                viewModel.ImageBaseUrl = _imageSettings.BaseUrl;

                return View(viewModel);
            }
        }

        // Displays detailed information about a specific movie
        [HttpGet]
        public async Task<IActionResult> Details(int id, int page = 1)
        {
            const int pageSize = 5;

            try
            {
                var movie = await _movieService.GetMovieByIdAsync(id);
                if (movie == null)
                {
                    TempData["Error"] = "Filme não encontrado!";
                    return RedirectToAction("Popular");
                }

                var (comments, totalComments) = await _ratingService.GetMovieCommentsAsync(id, page, pageSize);
                var (averageRating, totalRatings) = await _ratingService.GetMovieRatingStatsAsync(id);

                var commentViewModels = comments.Select(c => new MovieCommentItemViewModel
                {
                    Id = c.Id,
                    UserName = c.User.Name,
                    Rating = c.Rating,
                    Comment = c.Comment ?? "",
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                }).ToList();

                var commentsViewModel = new MovieCommentsViewModel
                {
                    Comments = commentViewModels,
                    CurrentPage = page,
                    TotalPages = (int)Math.Ceiling((double)totalComments / pageSize),
                    TotalComments = totalComments,
                    MovieId = id,
                    MovieTitle = movie.Title
                };

                var viewModel = new MovieDetailsViewModel
                {
                    Movie = movie,
                    ImageBaseUrl = _imageSettings.BaseUrl,
                    Comments = commentsViewModel,
                    AverageUserRating = averageRating,
                    TotalUserRatings = totalRatings
                };

                return View(viewModel);
            }
            catch (Exception)
            {
                TempData["Error"] = "Erro ao carregar detalhes do filme. Tente novamente mais tarde.";
                return RedirectToAction("Popular");
            }
        }
    }
}