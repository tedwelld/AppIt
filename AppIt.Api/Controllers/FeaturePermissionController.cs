using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeaturePermissionController : ControllerBase
    {
        private readonly IFeaturePermissionService _service;

        public FeaturePermissionController(IFeaturePermissionService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var fps = await _service.GetAllAsync();
            return Ok(fps);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var fp = await _service.GetByIdAsync(id);
            if (fp == null) return NotFound();
            return Ok(fp);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFeaturePermissionDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var fp = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = fp.FeaturePermissionId }, fp);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateFeaturePermissionDto dto)
        {
            if (id != dto.FeaturePermissionId) return BadRequest("ID mismatch");

            var fp = await _service.UpdateAsync(dto);
            if (fp == null) return NotFound();
            return Ok(fp);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
