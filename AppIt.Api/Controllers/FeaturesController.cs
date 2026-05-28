using AppIt.Api.Infrastructure;
using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "super,admin")]
    [Route("api/[controller]")]
    [Route("api/features")]
    public class FeatureController : ControllerBase
    {
        private readonly IFeatureService _service;

        public FeatureController(IFeatureService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ListQueryOptions query)
        {
            var features = await _service.GetAllAsync();
            return Ok(features.ApplyQuery(query,
                nameof(FeatureReadDto.Name),
                nameof(FeatureReadDto.Description)));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var feature = await _service.GetByIdAsync(id);
            if (feature == null) return NotFound();
            return Ok(feature);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFeatureDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var feature = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = feature.Id }, feature);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateFeatureDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");

            var feature = await _service.UpdateAsync(dto);
            if (feature == null) return NotFound();
            return Ok(feature);
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
