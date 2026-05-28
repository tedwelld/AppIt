using AppIt.Api.Infrastructure;
using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AppIt.Api.Controllers
{
    [Route("api/currencies")]
    [ApiController]
    [Authorize(Roles = "super,admin")]
    public class CurrenciesController : ControllerBase
    {
        private readonly ICurrencyService _service;

        public CurrenciesController(ICurrencyService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ListQueryOptions query)
        {
            var currencies = await _service.GetAllCurrenciesAsync();
            return Ok(currencies.ApplyQuery(query,
                nameof(CurrencyDto.Name),
                nameof(CurrencyDto.Code)));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var currency = await _service.GetCurrencyByIdAsync(id);
            if (currency == null) return NotFound();
            return Ok(currency);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCurrencyDto dto)
        {
            var currency = await _service.CreateCurrencyAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = currency.Id }, currency);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateCurrencyDto dto)
        {
            var currency = await _service.UpdateCurrencyAsync(id, dto);
            if (currency == null) return NotFound();
            return Ok(currency);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteCurrencyAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
