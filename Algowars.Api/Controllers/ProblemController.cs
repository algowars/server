
using Microsoft.AspNetCore.Mvc;
namespace Algowars.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class ProblemController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetProblems()
    {
        return Ok();
    }
}
