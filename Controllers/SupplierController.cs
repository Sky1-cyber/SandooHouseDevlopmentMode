using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sandoohouse.Controllers;

[Authorize]
public class SupplierController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
    
    public IActionResult AddSupplier()
    {
        return View();
    }
}