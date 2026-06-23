using AppIt.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "super,admin")]
    [Route("api/fiscal")]
    public class FiscalController : ControllerBase
    {
        private readonly IFiscalService _service;

        public FiscalController(IFiscalService service) => _service = service;

        [HttpGet("status")]
        public async Task<IActionResult> Status() => Ok(await _service.GetStatusAsync());

        [HttpPost("initialize")]
        public async Task<IActionResult> Initialize()
        {
            await _service.InitializeDeviceAsync();
            return Ok();
        }

        [HttpPost("run-invoices")]
        public async Task<IActionResult> RunInvoices() =>
            Ok(new { processed = await _service.FiscalizePendingInvoicesAsync() });

        [HttpPost("run-credit-notes")]
        public async Task<IActionResult> RunCreditNotes() =>
            Ok(new { processed = await _service.FiscalizePendingCreditNotesAsync() });
    }
}
