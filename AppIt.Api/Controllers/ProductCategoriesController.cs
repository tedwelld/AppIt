using AppIt.Api.Infrastructure;
using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/product-categories")]
    public class ProductCategoriesController : ControllerBase
    {
        private readonly IProductCategoryService _service;

        public ProductCategoriesController(IProductCategoryService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ListQueryOptions query)
        {
            var items = await _service.GetAllAsync();
            return Ok(items.ApplyQuery(query, nameof(ProductCategoryReadDto.Name)));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpPost]
        [Authorize(Roles = "super,admin")]
        public async Task<IActionResult> Create([FromBody] CreateProductCategoryDto dto)
        {
            var item = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "super,admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductCategoryDto dto)
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

    [ApiController]
    [Authorize]
    [Route("api/product-sub-categories")]
    public class ProductSubCategoriesController : ControllerBase
    {
        private readonly IProductSubCategoryService _service;

        public ProductSubCategoriesController(IProductSubCategoryService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ListQueryOptions query)
        {
            var items = await _service.GetAllAsync();
            return Ok(items.ApplyQuery(query, nameof(ProductSubCategoryReadDto.Name)));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpPost]
        [Authorize(Roles = "super,admin")]
        public async Task<IActionResult> Create([FromBody] CreateProductSubCategoryDto dto)
        {
            var item = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "super,admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductSubCategoryDto dto)
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
