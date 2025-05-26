using Microsoft.AspNetCore.Mvc;

namespace CineHub.Controllers
{
    public class BaseController : Controller
    {
        protected void ShowToast(string message, string type = "info")
        {
            TempData["ToastMessage"] = message;
            TempData["ToastType"] = type;
        }

        protected void ShowSuccess(string message)
        {
            ShowToast(message, "success");
        }

        protected void ShowError(string message)
        {
            ShowToast(message, "error");
        }

        protected void ShowWarning(string message)
        {
            ShowToast(message, "warning");
        }

        protected void ShowInfo(string message)
        {
            ShowToast(message, "info");
        }

        protected bool IsUserLoggedIn()
        {
            return HttpContext.Session.GetInt32("UserId") != null;
        }

        protected int GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 0;
        }
    }
}
