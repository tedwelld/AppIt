using AppIt.Api.Infrastructure;
using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers
{
    [ApiController]
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
                nameof(AuditLogReadDto.PerformedBy)));
        }
    }
}
