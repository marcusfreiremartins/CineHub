using Microsoft.AspNetCore.Mvc;
using CineHub.Services;
using CineHub.Models.ViewModels;
using CineHub.Configuration;
using Microsoft.Extensions.Options;
using CineHub.Models;
using CineHub.Models.ViewModels.Movies;

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
        public async Task<IActionResult> RateMovie(int movieId, string returnUrl = null)
        {
            if (!IsUserLoggedIn())
            {
                ShowWarning("Você precisa fazer login para avaliar filmes! 🔑");
                TempData["PendingMovieId"] = movieId;
                var loginReturnUrl = Url.Action("RateMovie", "Rating", new { movieId = movieId, returnUrl = returnUrl });
                return RedirectToAction("Login", "Auth", new { returnUrl = loginReturnUrl });
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
                UserRatingId = existingRating?.Id,
                ImageBaseUrl = _imageSettings.BaseUrl,
                ReturnUrl = returnUrl
            };

            return View("~/Views/Rating/RateMovie.cshtml", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> RateMovie(RateMovieViewModel model)
        {
            if (!IsUserLoggedIn())
            {
                ShowWarning("Sessão expirada. Faça login novamente.");
                return RedirectToAction("Login", "Auth");
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
                return View("~/Views/Rating/RateMovie.cshtml", model);
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

                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
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

            return View("~/Views/Rating/RateMovie.cshtml", model);
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
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Você precisa estar logado." });
                }

                TempData["ToastMessage"] = "Você precisa estar logado.";
                TempData["ToastType"] = "error";
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var userId = GetCurrentUserId();
                var result = await _ratingService.DeleteRating(userId, movieId);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = result.Success, message = result.Message });
                }

                TempData["ToastMessage"] = result.Message;
                TempData["ToastType"] = result.Success ? "success" : "error";
                return RedirectToAction("MyRatings", "User");
            }
            catch (Exception)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Erro ao excluir avaliação." });
                }

                TempData["ToastMessage"] = "Erro ao excluir avaliação.";
                TempData["ToastType"] = "error";
                return RedirectToAction("MyRatings", "User");
            }
        }

    }
}