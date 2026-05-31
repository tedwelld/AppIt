using AppIt.Api.Infrastructure;
using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AppIt.Api.Controllers
{
    [Route("api/exchange-rates")]
    [ApiController]
    [Authorize]
    public class ExchangeRatesController : ControllerBase
    {
        private readonly IExchangeRateService _service;

        public ExchangeRatesController(IExchangeRateService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ListQueryOptions query)
        {
            var rates = await _service.GetAllExchangeRatesAsync();
            return Ok(rates.ApplyQuery(query,
                nameof(ExchangeRateDto.CurrencyCode)));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var rate = await _service.GetExchangeRateByIdAsync(id);
            if (rate == null) return NotFound();
            return Ok(rate);
        }

        [HttpGet("by-date")]
        public async Task<IActionResult> GetByDate([FromQuery] DateTime date)
        {
            var rates = await _service.GetExchangeRatesByDateAsync(date);
            return Ok(rates);
        }

        [HttpGet("effective")]
        public async Task<IActionResult> GetEffective([FromQuery] DateTime? date)
        {
            var rates = await _service.GetEffectiveRatesAsync(date ?? DateTime.UtcNow);
            return Ok(rates);
        }

        [HttpGet("effective-rate")]
        public async Task<IActionResult> GetEffectiveRate([FromQuery] string currencyCode, [FromQuery] DateTime? date)
        {
            var rate = await _service.GetEffectiveRateAsync(currencyCode, date ?? DateTime.UtcNow);
            if (rate == null) return NotFound();
            return Ok(rate);
        }

        [HttpPost]
        [Authorize(Roles = "super,admin")]
        public async Task<IActionResult> Create(CreateExchangeRateDto dto)
        {
            var rate = await _service.CreateExchangeRateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = rate.Id }, rate);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "super,admin")]
        public async Task<IActionResult> Update(int id, UpdateExchangeRateDto dto)
        {
            var rate = await _service.UpdateExchangeRateAsync(id, dto);
            if (rate == null) return NotFound();
            return Ok(rate);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "super,admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteExchangeRateAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
