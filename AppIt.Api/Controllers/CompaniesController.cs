using AppIt.Core.Interfaces;
using AppIt.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _service;
    public CompaniesController(ICompanyService service) => _service = service;

    [HttpGet("details")]
    public async Task<IActionResult> Details() => Ok(await _service.CompanyDetailAsync());

    [HttpGet("list/{companyId}")]
    public async Task<IActionResult> List(int companyId) => Ok(await _service.CompanyListAsync(companyId));

    [HttpGet("dropdown/{companyId}")]
    public async Task<IActionResult> Dropdown(int companyId) => Ok(await _service.CompanyDropdownAsync(companyId));

    [HttpGet("summary/{companyId}")]
    public async Task<IActionResult> Summary(int companyId) => Ok(await _service.CompanySummary(companyId));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCompanyDto model) => Ok(await _service.CreatCompanyAsync(0, 0, 0));

    [HttpPut("{companyId}")]
    public async Task<IActionResult> Update(int companyId, [FromBody] UpdateCompanyDto model) => Ok(await _service.UpdateCompanyAsync(companyId, 0, 0));

    [HttpDelete("{companyId}")]
    public async Task<IActionResult> Delete(int companyId) => Ok(await _service.DeleteCompanyAsync(companyId, 0));
}
