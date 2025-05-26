using CineHub.Configuration;
using CineHub.Models;
using CineHub.Models.ViewModels;
using CineHub.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
namespace CineHub.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authService;
        private readonly RatingService _ratingService;
        private readonly ImageSettings _imageSettings;

        public AccountController(AuthService authService, RatingService ratingService, IOptions<ImageSettings> imageSettings)
        {
            _authService = authService;
            _ratingService = ratingService;
            _imageSettings = imageSettings.Value;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (IsUserLoggedIn())
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View("~/Views/User/Login.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.LoginAsync(model.Email, model.Password);

            if (result.Success && result.User != null)
            {
                // Salvar dados do usuário na sessão
                HttpContext.Session.SetInt32("UserId", result.User.Id);
                HttpContext.Session.SetString("UserName", result.User.Name);
                HttpContext.Session.SetString("UserEmail", result.User.Email);

                TempData["SuccessMessage"] = result.Message;

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", result.Message);
            return View("~/Views/User/Login.cshtml", model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (IsUserLoggedIn())
            {
                return RedirectToAction("Index", "Home");
            }

            return View("~/Views/User/Register.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/User/Register.cshtml", model);
            }

            var result = await _authService.RegisterAsync(model.Name, model.Email, model.Password);

            if (result.Success && result.User != null)
            {
                // Login automático após registro
                HttpContext.Session.SetInt32("UserId", result.User.Id);
                HttpContext.Session.SetString("UserName", result.User.Name);
                HttpContext.Session.SetString("UserEmail", result.User.Email);

                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", result.Message);
            return View("~/Views/User/Register.cshtml", model);
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Logout realizado com sucesso!";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login");
            }

            var userId = GetCurrentUserId();
            var user = await _authService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return RedirectToAction("Login");
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


        [HttpGet]
        public async Task<IActionResult> Favorites()
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login");
            }

            var userId = GetCurrentUserId();
            var favorites = await _ratingService.GetUserFavoritesAsync(userId);

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

        [HttpGet]
        public async Task<IActionResult> MyRatings()
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login");
            }

            var userId = GetCurrentUserId();
            var ratings = await _ratingService.GetUserRatingsAsync(userId, 50);

            var viewModelList = ratings.Select(r => new RatingItemViewModel
            {
                Id = r.Id,
                MovieId = r.Movie.Id,
                MovieTitle = r.Movie.Title,
                MoviePosterPath = r.Movie.PosterPath,
                MovieOverview = r.Movie.Overview,
                UserRating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            }).ToList();

            ViewBag.ImageBaseUrl = _imageSettings.BaseUrl;
            return View("~/Views/User/MyRatings.cshtml", viewModelList);
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