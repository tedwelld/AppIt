using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using AppIt.Core.Interfaces.Services;
using AppIt.Data.Entities.AppIt.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportSnapshotsController : ControllerBase
    {
        private readonly IReportSnapshotService _service;

        public ReportSnapshotsController(IReportSnapshotService service)
        {
            _service = service;
        }

        [HttpGet("report/{reportKey}")]
        public async Task<IActionResult> GetByReportKey(string reportKey)
        {
            var snapshots = await _service.GetByReportKeyAsync(reportKey);
            return Ok(snapshots);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var snapshot = await _service.GetByIdAsync(id);

            if (snapshot == null)
                return NotFound();

            return Ok(snapshot);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateReportSnapshotDto dto)
        {
            var id = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id }, null);
        }
    }
}
