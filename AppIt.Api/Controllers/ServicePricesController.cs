using AppIt.Api.Infrastructure;
using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/service-prices")]
    public class ServicePricesController : ControllerBase
    {
        private readonly IServicePriceService _service;

        public ServicePricesController(IServicePriceService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ListQueryOptions query)
        {
            var items = await _service.GetAllAsync();
            return Ok(items.ApplyQuery(query,
                nameof(ServicePriceReadDto.ServiceType),
                nameof(ServicePriceReadDto.CurrencyCode)));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpGet("by-service/{serviceType}/{serviceId:int}")]
        public async Task<IActionResult> GetByService(string serviceType, int serviceId)
        {
            var items = await _service.GetByServiceAsync(serviceType, serviceId);
            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateServicePriceDto dto)
        {
            var item = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateServicePriceDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            var item = await _service.UpdateAsync(dto);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}
