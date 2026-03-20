using Microsoft.AspNetCore.Mvc;

namespace Sandoohouse.Controllers;

public class SaleController : Controller
{
    // GET
    public IActionResult Index()
    {
        return RedirectToAction("Index", "ComingSoon");
    }
}