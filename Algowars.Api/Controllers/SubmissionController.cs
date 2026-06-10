using Microsoft.AspNetCore.Mvc;

namespace Algowars.Api.Controllers;

public class SubmissionController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
