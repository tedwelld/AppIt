using AppIt.Core.Interfaces;
using AppIt.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoleFeaturesController : ControllerBase
{
    public readonly IRoleFeatureService _service;
    public RoleFeaturesController(IRoleFeatureService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _service.GetRoleFeaturesAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RoleFeatureDto dto) => Ok(await _service.CreateRoleFeatureAsync(dto));

    [HttpPut("{roleId}/{featureId}")]
    public async Task<IActionResult> Update(int roleId, int featureId, [FromBody] RoleFeatureDto dto) => Ok(await _service.UpdateRoleFeatureAsync(roleId, featureId, dto));

    [HttpDelete("{roleId}/{featureId}")]
    public async Task<IActionResult> Delete(int roleId, int featureId) => Ok(await _service.DeleteRoleFeatureAsync(roleId, featureId));
}
