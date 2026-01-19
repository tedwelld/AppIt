using AppIt.Core.Interfaces;
using AppIt.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountCategoriesController : ControllerBase
{
    private readonly IAccountCategoryService _service;
    public AccountCategoriesController(IAccountCategoryService service) => _service = service;

    [HttpGet("features")]
    public async Task<IActionResult> Features() => Ok(await _service.GetAllFeatures());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) => Ok(await _service.GetAccountCategoryById(id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAccountCategoryDto dto) => Ok(await _service.GetAccountCategoryById(0, dto.Name));

    [HttpPut]
    public async Task<IActionResult> Update() => Ok(await _service.UpdateAccountCategory());

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) => Ok(await _service.DeleteAccountCategory(id));
}
