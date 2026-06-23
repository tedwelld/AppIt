using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "super,admin")]
    [Route("api/credit-memos")]
    public class CreditMemosController : ControllerBase
    {
        private readonly ICreditMemoService _service;

        public CreditMemosController(ICreditMemoService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCreditMemoDto dto) =>
            Ok(await _service.CreateAsync(dto));
    }
}
