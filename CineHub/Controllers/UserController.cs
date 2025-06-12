using CineHub.Configuration;
using CineHub.Models.ViewModels.User;
using CineHub.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CineHub.Controllers
{
    public class UserController : BaseController
    {
        private readonly AuthService _authService;
        private readonly RatingService _ratingService;
        private readonly ImageSettings _imageSettings;

        public UserController(AuthService authService, RatingService ratingService, IOptions<ImageSettings> imageSettings)
        {
            _authService = authService;
            _ratingService = ratingService;
            _imageSettings = imageSettings.Value;
        }

        // Displays the user's profile information and recent activity
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (!IsUserLoggedIn())
            {
                ShowWarning("Você precisa fazer login para acessar seu perfil.");
                return RedirectToAction("Login", "Auth");
            }

            var userId = GetCurrentUserId();
            var user = await _authService.GetUserByIdAsync(userId);

            if (user == null)
            {
                ShowError("Erro ao carregar perfil do usuário.");
                return RedirectToAction("Login", "Auth");
            }

            var (averageRating, totalRatings) = await _ratingService.GetUserRatingStatsAsync(userId);
            var recentRatings = await _ratingService.GetUserRatingsAsync(userId, 5);
            var recentFavorites = await _ratingService.GetUserFavoritesAsync(userId);

            var viewModel = new UserProfileViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                TotalRatings = totalRatings,
                TotalFavorites = recentFavorites.Count,
                AverageRating = averageRating,
                RecentRatings = recentRatings,
                RecentFavorites = recentFavorites.Take(5).ToList(),
                ImageBaseUrl = _imageSettings.BaseUrl
            };

            return View("~/Views/User/UserProfile.cshtml", viewModel);
        }

        // Shows a list of the user's favorite movies
        [HttpGet]
        public async Task<IActionResult> Favorites(bool deleted = false, string? message = null)
        {
            if (!IsUserLoggedIn())
            {
                ShowWarning("Você precisa fazer login para ver seus favoritos.");
                return RedirectToAction("Login", "Auth");
            }

            var userId = GetCurrentUserId();
            var favorites = await _ratingService.GetUserFavoritesAsync(userId);

            if (!favorites.Any())
            {
                ShowInfo("Você ainda não tem filmes favoritos. Que tal explorar alguns filmes? 🎬");
            }

            if (deleted && !string.IsNullOrEmpty(message))
            {
                ShowSuccess(message);
            }

            var favoriteItems = favorites.Select(f => new FavoriteItemViewModel
            {
                Id = f.Id,
                MovieId = f.Movie.Id,
                MovieTitle = f.Movie.Title,
                MoviePosterPath = f.Movie.PosterPath,
                MovieOverview = f.Movie.Overview,
                AddedAt = f.CreatedAt
            }).ToList();

            ViewBag.ImageBaseUrl = _imageSettings.BaseUrl;
            return View("~/Views/User/Favorites.cshtml", favoriteItems);
        }

        // Displays all movies rated by the user
        [HttpGet]
        public async Task<IActionResult> MyRatings(bool deleted = false, string? message = null)
        {
            if (!IsUserLoggedIn())
            {
                ShowWarning("Você precisa fazer login para ver suas avaliações.");
                return RedirectToAction("Login", "Auth");
            }

            var userId = GetCurrentUserId();
            var ratings = await _ratingService.GetUserRatingsAsync(userId, 50);

            if (!ratings.Any())
            {
                ShowInfo("Você ainda não avaliou nenhum filme. Que tal começar agora? ⭐");
            }

            if (deleted && !string.IsNullOrEmpty(message))
            {
                ShowSuccess(message);
            }

            var viewModelList = ratings.Select(r => new RatingItemViewModel
            {
                Id = r.Id,
                MovieId = r.Movie.Id,
                MovieTitle = r.Movie.Title,
                MoviePosterPath = r.Movie.PosterPath,
                MovieOverview = r.Movie.Overview,
                UserRating = r.Rating,
                Comment = r.Comment,
                LastActivityDate = r.LastActivityDate,
                UpdatedAt = r.UpdatedAt
            }).ToList();

            ViewBag.ImageBaseUrl = _imageSettings.BaseUrl;
            return View("~/Views/User/MyRatings.cshtml", viewModelList);
        }
    }
}