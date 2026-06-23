using AppIt.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "super,admin")]
    [Route("api/night-audit")]
    public class NightAuditController : ControllerBase
    {
        private readonly INightAuditService _service;

        public NightAuditController(INightAuditService service) => _service = service;

        [HttpPost("process")]
        public async Task<IActionResult> Process([FromQuery] DateTime? date) =>
            Ok(new { processed = await _service.ProcessReservationProductsAsync(date) });
    }
}
