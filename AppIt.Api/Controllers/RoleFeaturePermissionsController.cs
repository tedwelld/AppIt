using AppIt.Api.Infrastructure;
using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers;

[ApiController]
[Authorize(Roles = "super,admin")]
[Route("api/role-feature-permissions")]
public class RoleFeaturePermissionsController : ControllerBase
{
    private readonly IRoleFeatureService _service;

    public RoleFeaturePermissionsController(IRoleFeatureService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ListQueryOptions query)
    {
        var result = await _service.GetRoleFeaturePermissionsAsync();
        if (!result.Success)
        {
            return BadRequest(result);
        }

        var paged = (result.Data ?? new List<RoleFeaturePermissionDto>()).ApplyQuery(query,
            nameof(RoleFeaturePermissionDto.RoleId),
            nameof(RoleFeaturePermissionDto.FeatureId),
            nameof(RoleFeaturePermissionDto.PermissionId));

        return Ok(new ServiceResponse<PagedResult<RoleFeaturePermissionDto>>
        {
            Data = paged,
            Success = true,
            Message = result.Message,
            Time = result.Time
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RoleFeaturePermissionDto dto) =>
        Ok(await _service.CreateRoleFeaturePermissionAsync(dto));

    [HttpPut("{roleId}/{featureId}/{permissionId}")]
    public async Task<IActionResult> Update(int roleId, int featureId, int permissionId, [FromBody] RoleFeaturePermissionDto dto) =>
        Ok(await _service.UpdateRoleFeaturePermissionAsync(roleId, featureId, permissionId, dto));

    [HttpDelete("{roleId}/{featureId}/{permissionId}")]
    public async Task<IActionResult> Delete(int roleId, int featureId, int permissionId) =>
        Ok(await _service.DeleteRoleFeaturePermissionAsync(roleId, featureId, permissionId));
}
