using Microsoft.AspNetCore.Mvc;

public class BaseController : Controller
{
    // Shows a toast notification with a specific message and type
    protected void ShowToast(string message, string type = "info")
    {
        TempData["ToastMessage"] = message;
        TempData["ToastType"] = type;
    }

    // Shows a success toast notification
    protected void ShowSuccess(string message)
    {
        ShowToast(message, "success");
    }

    // Shows an error toast notification
    protected void ShowError(string message)
    {
        ShowToast(message, "error");
    }

    // Shows a warning toast notification
    protected void ShowWarning(string message)
    {
        ShowToast(message, "warning");
    }

    // Shows an info toast notification
    protected void ShowInfo(string message)
    {
        ShowToast(message, "info");
    }

    // Checks if a user is currently logged in
    protected bool IsUserLoggedIn()
    {
        return HttpContext.Session.GetInt32("UserId") != null;
    }

    // Retrieves the ID of the currently logged-in user
    protected int GetCurrentUserId()
    {
        return HttpContext.Session.GetInt32("UserId") ?? 0;
    }
}