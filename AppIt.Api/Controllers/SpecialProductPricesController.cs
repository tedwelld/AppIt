using AppIt.Api.Infrastructure;
using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/special-product-prices")]
    public class SpecialProductPricesController : ControllerBase
    {
        private readonly ISpecialProductPriceService _service;

        public SpecialProductPricesController(ISpecialProductPriceService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ListQueryOptions query)
        {
            var items = await _service.GetAllAsync();
            return Ok(items.ApplyQuery(query,
                nameof(SpecialProductPriceReadDto.CurrencyCode),
                nameof(SpecialProductPriceReadDto.ProductType)));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpGet("by-product/{productId:int}")]
        public async Task<IActionResult> GetByProduct(int productId)
            => Ok(await _service.GetByProductAsync(productId));

        [HttpPost]
        [Authorize(Roles = "super,admin")]
        public async Task<IActionResult> Create([FromBody] CreateSpecialProductPriceDto dto)
        {
            var item = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "super,admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSpecialProductPriceDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            var item = await _service.UpdateAsync(dto);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "super,admin")]
        public async Task<IActionResult> Delete(int id)
            => await _service.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
