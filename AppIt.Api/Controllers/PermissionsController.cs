using AppIt.Core.Interfaces;
using AppIt.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionService _service;
    public PermissionsController(IPermissionService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _service.GetPermissionAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) => Ok(await _service.GetPermissionByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePermissionDto dto) => Ok(await _service.CreatePermissionAsync(dto));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePermissionDto dto) => Ok(await _service.UpdatePermissionAsync(id, dto));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) => Ok(await _service.DeletePermissionAsync(id));
}
