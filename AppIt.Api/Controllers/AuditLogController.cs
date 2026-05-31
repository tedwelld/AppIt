using AppIt.Api.Infrastructure;
using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "super,admin")]
    [Route("api/audit-logs")]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogService _service;

        public AuditLogsController(IAuditLogService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ListQueryOptions query)
        {
            var logs = await _service.GetAllAsync();
            return Ok(logs.ApplyQuery(query,
                nameof(AuditLogReadDto.Action),
                nameof(AuditLogReadDto.EntityName),
                nameof(AuditLogReadDto.PerformedBy),
                nameof(AuditLogReadDto.PerformedAt)));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var log = await _service.GetByIdAsync(id);
            return log == null ? NotFound() : Ok(log);
        }
    }
}
