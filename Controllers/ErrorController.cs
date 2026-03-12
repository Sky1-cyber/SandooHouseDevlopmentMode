using Microsoft.AspNetCore.Mvc;

namespace Sandoohouse.Controllers;

public class ErrorController : Controller
{
    public IActionResult AccessDenied()
    {
        return View();
    }
}