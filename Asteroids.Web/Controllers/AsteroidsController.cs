using Asteroids.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Asteroids.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AsteroidsController : ControllerBase
    {
        private readonly INeoService _neoClientService;

        public AsteroidsController(INeoService neoClientService)
        {
            _neoClientService = neoClientService;
        }

        [HttpGet("get")]
        public async Task<IActionResult> Get(int? days)
        {
            if (days < 0 || days == null)
            {
                return BadRequest("The number of days must be positive.");
            }

            return Ok(await _neoClientService.GetNeoListAsync(days.Value));
        }
    }
}
