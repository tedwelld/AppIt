using AppIt.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "super,admin")]
    [Route("api/trial-balance")]
    public class TrialBalanceController : ControllerBase
    {
        private readonly ITrialBalanceService _service;

        public TrialBalanceController(ITrialBalanceService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] DateTime? asOf) =>
            Ok(await _service.GetTrialBalanceAsync(asOf));
    }
}
