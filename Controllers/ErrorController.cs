using Microsoft.AspNetCore.Mvc;

namespace Sandoohouse.Controllers;
public class ErrorController : Controller
{
    [Route("Error/{statusCode}")]
    public IActionResult HandleErrorCode(int statusCode)
    {
        if (statusCode == 404)
        {
            return View("NotFound"); // Views/Error/NotFound.cshtml
        }

        if (statusCode == 403)
        {
            return View("AccessDenied");
        }

        return View("Error"); // fallback
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
}

    
