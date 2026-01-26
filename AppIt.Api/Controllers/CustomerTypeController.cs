using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerTypeController : ControllerBase
    {
        private readonly ICustomerTypeService _service;

        public CustomerTypeController(ICustomerTypeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var customerTypes = await _service.GetAllAsync();
            return Ok(customerTypes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var customerType = await _service.GetByIdAsync(id);
            if (customerType == null) return NotFound();
            return Ok(customerType);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomerTypeDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var customerType = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = customerType.Id }, customerType);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerTypeDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");

            var customerType = await _service.UpdateAsync(dto);
            if (customerType == null) return NotFound();
            return Ok(customerType);
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
