using Microsoft.AspNetCore.Mvc;

namespace Sandoohouse.Controllers;

public class ComingSoonController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}