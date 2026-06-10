using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Algowars.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class AccountController : Controller
{
    [HttpPut]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult UpdateAccount()
    {
        return Ok();
    }
}
