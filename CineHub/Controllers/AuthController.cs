using CineHub.Models.ViewModels;
using CineHub.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using CineHub.Configuration;
using CineHub.Models.ViewModels.Account;

namespace CineHub.Controllers
{
    public class AuthController : BaseController
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (IsUserLoggedIn())
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View("~/Views/Account/Login.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Account/Login.cshtml", model);
            }

            var result = await _authService.LoginAsync(model.Email, model.Password);

            if (result.Success && result.User != null)
            {
                HttpContext.Session.SetInt32("UserId", result.User.Id);
                HttpContext.Session.SetString("UserName", result.User.Name);
                HttpContext.Session.SetString("UserEmail", result.User.Email);

                ShowSuccess($"Bem-vindo de volta, {result.User.Name}! 🎬");

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", result.Message);
            return View("~/Views/Account/Login.cshtml", model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (IsUserLoggedIn())
            {
                return RedirectToAction("Index", "Home");
            }

            return View("~/Views/Account/Register.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Account/Register.cshtml", model);
            }

            var result = await _authService.RegisterAsync(model.Name, model.Email, model.Password);

            if (result.Success && result.User != null)
            {
                HttpContext.Session.SetInt32("UserId", result.User.Id);
                HttpContext.Session.SetString("UserName", result.User.Name);
                HttpContext.Session.SetString("UserEmail", result.User.Email);

                ShowSuccess($"Conta criada com sucesso! Bem-vindo ao CineHub, {result.User.Name}! 🎉");
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", result.Message);
            return View("~/Views/Account/Register.cshtml", model);
        }

        [HttpPost]
        public IActionResult Logout()
        {
            var userName = HttpContext.Session.GetString("UserName");
            HttpContext.Session.Clear();
            ShowInfo($"Até logo, {userName}! Volte sempre! 👋");
            return RedirectToAction("Index", "Home");
        }
    }
}
