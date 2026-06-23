using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "super,admin")]
    [Route("api/hconnect")]
    public class HConnectController : ControllerBase
    {
        private readonly IHConnectService _service;

        public HConnectController(IHConnectService service) => _service = service;

        [HttpGet("bookings")]
        public async Task<IActionResult> Pending() => Ok(await _service.GetPendingAsync());

        [HttpPost("reservations/{id:int}/queue")]
        public async Task<IActionResult> Queue(int id)
        {
            var result = await _service.QueueReservationAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost("retry")]
        public async Task<IActionResult> Retry() =>
            Ok(new { processed = await _service.ProcessPendingRetriesAsync() });
    }

    [ApiController]
    [Authorize]
    [Route("api/occupancy")]
    public class OccupancyController : ControllerBase
    {
        [HttpGet("calendar")]
        public IActionResult Calendar() => Ok(Array.Empty<object>());
    }

    [ApiController]
    [Authorize]
    [Route("api/village")]
    public class VillageController : ControllerBase
    {
        private readonly IBeds24Service _beds24;

        public VillageController(IBeds24Service beds24) => _beds24 = beds24;

        [HttpGet("availability")]
        public async Task<IActionResult> Availability()
        {
            await _beds24.CallApiAsync();
            return Ok(Array.Empty<object>());
        }
    }

    [ApiController]
    [Authorize(Roles = "super,admin")]
    [Route("api/product-price-agent")]
    public class ProductPriceAgentController : ControllerBase
    {
        private readonly IProductPriceAgentService _service;

        public ProductPriceAgentController(IProductPriceAgentService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? companyId, [FromQuery] int? year) =>
            Ok(await _service.GetAllAsync(companyId, year));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAgentProductPriceDto dto) =>
            Ok(await _service.CreateAsync(dto));

        [HttpPut("{id:int}/verify")]
        public async Task<IActionResult> Verify(int id) =>
            Ok(await _service.VerifyAsync(id, User.Identity?.Name ?? "system"));

        [HttpPut("{id:int}/approve")]
        public async Task<IActionResult> Approve(int id) =>
            Ok(await _service.ApproveAsync(id, User.Identity?.Name ?? "system"));

        [HttpPut("{id:int}/send-to-agent")]
        public async Task<IActionResult> Send(int id) =>
            Ok(await _service.SendToAgentAsync(id));

        [HttpPut("agent-approval")]
        [AllowAnonymous]
        public async Task<IActionResult> AgentApproval([FromBody] AgentApprovalDto dto) =>
            Ok(await _service.AgentApprovalAsync(dto));
    }

    [ApiController]
    [Authorize]
    [Route("api/reports")]
    public class AllReportsController : ControllerBase
    {
        [HttpGet]
        public IActionResult List() => Ok(new[] { new { id = "reservations", name = "Reservations" } });
    }

    [ApiController]
    [Authorize]
    [Route("api/reporting")]
    public class ReportingController : ControllerBase
    {
        [HttpGet("health")]
        public IActionResult Health() => Ok(new { status = "ok", engine = "AppIt.Report stub" });
    }
}
