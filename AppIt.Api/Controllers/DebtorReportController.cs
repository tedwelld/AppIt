using AppIt.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "super,admin")]
    [Route("api/debtor-reports")]
    public class DebtorReportController : ControllerBase
    {
        private readonly IDebtorReportService _service;

        public DebtorReportController(IDebtorReportService service) => _service = service;

        [HttpGet("aging")]
        public async Task<IActionResult> Aging([FromQuery] int? companyId) =>
            Ok(await _service.GetAgingAsync(companyId));
    }
}
