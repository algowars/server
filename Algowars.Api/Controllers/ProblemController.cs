using Microsoft.AspNetCore.Mvc;

namespace Algowars.Api.Controllers;

public class ProblemController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
