using AppIt.Core.Interfaces;
using AppIt.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _service;
    public RolesController(IRoleService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _service.GetRolesAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) => Ok(await _service.GetRoleByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RoleDto dto) => Ok(await _service.CreateRoleAsync(dto));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] RoleDto dto) => Ok(await _service.UpdateRoleAsync(id, dto));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) => Ok(await _service.DeleteRoleAsync(id));
}
