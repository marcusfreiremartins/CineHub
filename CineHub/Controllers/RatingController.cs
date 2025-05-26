using Microsoft.AspNetCore.Mvc;
using CineHub.Services;
using CineHub.Models.ViewModels;
using CineHub.Configuration;
using Microsoft.Extensions.Options;
using CineHub.Models;

namespace CineHub.Controllers
{
    public class RatingController : BaseController
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
                ShowWarning("Você precisa fazer login para avaliar filmes! 🔑");
                TempData["PendingMovieId"] = movieId;
                var returnUrl = Url.Action("RateMovie", "Rating", new { movieId = movieId });
                return RedirectToAction("Login", "Account", new { returnUrl = returnUrl });
            }

            var movie = await _movieService.GetMovieByIdAsync(movieId);
            if (movie == null)
            {
                ShowError("Filme não encontrado!");
                return RedirectToAction("Index", "Movies");
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
                ShowWarning("Sessão expirada. Faça login novamente.");
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
                ShowError("Verifique os dados informados.");
                return View("~/Views/User/RateMovie.cshtml", model);
            }

            var userId = GetCurrentUserId();

            try
            {
                await _ratingService.ToggleFavoriteSilentlyAsync(userId, model.MovieId, model.IsFavorite);
                var result = await _ratingService.RateMovieAsync(userId, model.MovieId, model.Rating, model.Comment);

                if (result.Success)
                {
                    if (model.IsFavorite)
                    {
                        ShowSuccess($"Avaliação salva e filme adicionado aos favoritos! ⭐❤️");
                    }
                    else
                    {
                        ShowSuccess("Avaliação salva com sucesso! ⭐");
                    }
                    return RedirectToAction("Details", "Movies", new { id = model.MovieId });
                }

                ModelState.AddModelError("", result.Message);
                ShowError(result.Message);
            }
            catch (Exception)
            {
                ShowError("Erro inesperado ao salvar avaliação. Tente novamente.");
            }

            return View("~/Views/User/RateMovie.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int movieId)
        {
            if (!IsUserLoggedIn())
            {
                return Json(new { success = false, message = "Você precisa estar logado para favoritar filmes." });
            }

            try
            {
                var userId = GetCurrentUserId();
                var result = await _ratingService.ToggleFavoriteAsync(userId, movieId);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Erro ao atualizar favoritos." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRating(int movieId)
        {
            if (!IsUserLoggedIn())
            {
                return Json(new { success = false, message = "Você precisa estar logado." });
            }

            try
            {
                var userId = GetCurrentUserId();
                var result = await _ratingService.DeleteRating(userId, movieId);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Erro ao excluir avaliação." });
            }
        }
    }
}