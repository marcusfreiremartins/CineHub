using Microsoft.AspNetCore.Mvc;
using CineHub.Services;
using CineHub.Models.ViewModels;
using CineHub.Configuration;
using Microsoft.Extensions.Options;
using CineHub.Models;

namespace CineHub.Controllers
{
    public class RatingController : Controller
    {
        private readonly RatingService _ratingService;
        private readonly MovieService _movieService;
        private readonly ImageSettings _imageSettings;

        public RatingController(RatingService ratingService, MovieService movieService, IOptions<ImageSettings> imageSettings)
        {
            _ratingService = ratingService;
            _movieService = movieService;
            _imageSettings = imageSettings.Value;
        }

        [HttpGet]
        public async Task<IActionResult> RateMovie(int movieId)
        {
            if (!IsUserLoggedIn())
            {
                TempData["LoginMessage"] = "Você precisa fazer login para avaliar filmes!";
                TempData["MessageType"] = "warning";
                TempData["PendingMovieId"] = movieId;

                var returnUrl = Url.Action("RateMovie", "Rating", new { movieId = movieId });

                return RedirectToAction("Login", "Account", new { returnUrl = returnUrl });
            }

            var movie = await _movieService.GetMovieByIdAsync(movieId);
            if (movie == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();
            var existingRating = await _ratingService.GetUserRatingAsync(userId, movieId);
            var isFavorite = await _ratingService.IsMovieFavoriteAsync(userId, movieId);

            var viewModel = new RateMovieViewModel
            {
                MovieId = movieId,
                MovieTitle = movie.Title,
                MoviePosterPath = movie.PosterPath,
                Rating = existingRating?.Rating ?? 5,
                Comment = existingRating?.Comment,
                IsFavorite = isFavorite,
                ExistingRatingId = existingRating?.Id,
                ImageBaseUrl = _imageSettings.BaseUrl
            };

            return View("~/Views/User/RateMovie.cshtml", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> RateMovie(RateMovieViewModel model)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                var movie = await _movieService.GetMovieByIdAsync(model.MovieId);
                if (movie != null)
                {
                    model.MovieTitle = movie.Title;
                    model.MoviePosterPath = movie.PosterPath;
                    model.ImageBaseUrl = _imageSettings.BaseUrl;
                }
                return View(model);
            }

            var userId = GetCurrentUserId();

            await _ratingService.ToggleFavoriteSilentlyAsync(userId, model.MovieId, model.IsFavorite);
            var result = await _ratingService.RateMovieAsync(userId, model.MovieId, model.Rating, model.Comment);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("Details", "Movies", new { id = model.MovieId });
            }

            ModelState.AddModelError("", result.Message);
            return View("~/Views/User/RateMovie.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int movieId)
        {
            if (!IsUserLoggedIn())
            {
                return Json(new { success = false, message = "Você precisa estar logado para favoritar filmes." });
            }

            var userId = GetCurrentUserId();
            var result = await _ratingService.ToggleFavoriteAsync(userId, movieId);

            return Json(new { success = result.Success, message = result.Message });
        }

        //Função comentado no momento, seria para mostrar todas as avaliações dos usuários no banco de dados
        //[HttpGet]
        //public async Task<IActionResult> MovieRatings(int movieId)
        //{
        //var movie = await _movieService.GetMovieByIdAsync(movieId);
        //if (movie == null)
        //{
        //return NotFound();
        //}

        //var ratings = await _ratingService.GetMovieRatingsAsync(movieId);
        //var averageRating = await _ratingService.GetMovieAverageRatingAsync(movieId);
        //var ratingCount = await _ratingService.GetMovieRatingCountAsync(movieId);

        //ViewBag.Movie = movie;
        //ViewBag.AverageRating = averageRating;
        //ViewBag.RatingCount = ratingCount;
        //ViewBag.ImageBaseUrl = _imageSettings.BaseUrl;

        //return View(ratings);
        //}

        [HttpPost]
        public async Task<IActionResult> DeleteRating(int movieId)
        {
            if (!IsUserLoggedIn())
            {
                return Json(new { success = false, message = "Você precisa estar logado." });
            }

            var userId = GetCurrentUserId();
            var result = await _ratingService.DeleteRating(userId, movieId);

            return Json(new { success = result.Success, message = result.Message });
        }

        // Helper methods
        private bool IsUserLoggedIn()
        {
            return HttpContext.Session.GetInt32("UserId") != null;
        }

        private int GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 0;
        }
    }
}