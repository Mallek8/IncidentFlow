using IncidentFlow.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IncidentFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(ApiResponse<object>.Ok(new
        {
            status = "Healthy",
            service = "IncidentFlow.API",
            utc = DateTime.UtcNow
        }));
    }
}