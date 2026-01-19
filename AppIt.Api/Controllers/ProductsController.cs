using AppIt.Core.Interfaces;
using AppIt.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;
    public ProductsController(IProductService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ProductFilterDto filter) => Ok(await _service.GetProductsAsync(filter));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) => Ok(await _service.GetProductByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto) => Ok(await _service.CreateProductAsync(dto));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto) => Ok(await _service.UpdateProductAsync(id, dto));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) => Ok(await _service.DeleteProductAsync(id));
}
