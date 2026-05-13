using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sandoohouse.ApplicationProgram;

namespace Sandoohouse.Controllers;

public class ExpensesController : Controller
{
    private readonly ApplicationDbContext _applicationDbContext;
    public ExpensesController(ApplicationDbContext context)
    {
        _applicationDbContext = context;
    }
        
    // GET
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Owner,Manager")]
    public IActionResult Index()
    {
        return View();
    }
}