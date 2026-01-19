using AppIt.Core.Interfaces;
using AppIt.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _service;
    public DepartmentsController(IDepartmentService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DepartmentFilterDto filter) => Ok(await _service.GetDepartmentsAsync(filter));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) => Ok(await _service.GetDepartmentByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto) => Ok(await _service.CreateDepartmentAsync(dto));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDepartmentDto dto) => Ok(await _service.UpdateDepartmentAsync(id, dto));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) => Ok(await _service.DeleteDepartmentAsync(id));
}
