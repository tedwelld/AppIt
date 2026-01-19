using AppIt.Core.Interfaces;
using AppIt.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeaturesController : ControllerBase
{
    private readonly IFeatureService _service;
    public FeaturesController(IFeatureService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _service.GetFeatureAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) => Ok(await _service.GetFeatureByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFeatureDto dto) => Ok(await _service.CreateFeatureAsync(dto));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateFeatureDto dto) => Ok(await _service.UpdateFeatureAsync(id, dto));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) => Ok(await _service.DeleteFeatureAsync(id));
}
