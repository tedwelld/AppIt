using AppIt.Api.Infrastructure;
using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers
{
    [ApiController]
    [Route("api/report-snapshots")]
    public class ReportSnapshotsController : ControllerBase
    {
        private readonly IReportSnapshotService _service;

        public ReportSnapshotsController(IReportSnapshotService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ListQueryOptions query)
        {
            var snapshots = await _service.GetByReportKeyAsync(string.Empty);
            return Ok(snapshots.ApplyQuery(query,
                nameof(ReportSnapshotDto.ReportKey),
                nameof(ReportSnapshotDto.Title)));
        }

        [HttpGet("report/{reportKey}")]
        public async Task<IActionResult> GetByReportKey(string reportKey, [FromQuery] ListQueryOptions query)
        {
            var snapshots = await _service.GetByReportKeyAsync(reportKey);
            return Ok(snapshots.ApplyQuery(query,
                nameof(ReportSnapshotDto.ReportKey),
                nameof(ReportSnapshotDto.Title)));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var snapshot = await _service.GetByIdAsync(id);
            if (snapshot == null)
            {
                return NotFound();
            }

            return Ok(snapshot);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateReportSnapshotDto dto)
        {
            var id = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id }, null);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
