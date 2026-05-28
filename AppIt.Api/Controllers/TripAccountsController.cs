using AppIt.Api.Infrastructure;
using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "super,admin")]
    [Route("api/trip-accounts")]
    public class TripAccountsController : ControllerBase
    {
        private readonly ICompanyService _companies;

        public TripAccountsController(ICompanyService companies)
        {
            _companies = companies;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ListQueryOptions query)
        {
            var rows = (await _companies.GetAllAsync()).Select(ToTripAccount);
            return Ok(rows.ApplyQuery(
                query,
                nameof(TripAccountReadDto.AgentName),
                nameof(TripAccountReadDto.AgentType),
                nameof(TripAccountReadDto.Email),
                nameof(TripAccountReadDto.Phone),
                nameof(TripAccountReadDto.RegistrationNumber)));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var company = await _companies.GetByIdAsync(id);
            return company == null ? NotFound() : Ok(ToTripAccount(company));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTripAccountDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var company = await _companies.CreateAsync(ToCreateCompany(dto));
            return CreatedAtAction(nameof(GetById), new { id = company.CompanyId }, ToTripAccount(company));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTripAccountDto dto)
        {
            if (id != dto.TripAccountId) return BadRequest("Trip account ID mismatch");

            var company = await _companies.UpdateAsync(ToUpdateCompany(dto));
            return company == null ? NotFound() : Ok(ToTripAccount(company));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _companies.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        private static TripAccountReadDto ToTripAccount(CompanyReadDto company) => new()
        {
            TripAccountId = company.CompanyId,
            AgentName = company.CompanyName,
            AgentType = company.AgentType,
            Email = company.CompanyEmail,
            Phone = company.CompanyPhone,
            Address = company.CompanyAddress,
            RegistrationNumber = company.RegNumber,
            AccountNumber = company.AccountNumber,
            VatNumber = company.VatNumber,
            CreatedDate = company.CreatedDate,
            UpdatedDate = company.UpdatedDate
        };

        private static CreateCompanyDto ToCreateCompany(CreateTripAccountDto dto) => new()
        {
            CompanyName = dto.AgentName,
            AgentType = string.IsNullOrWhiteSpace(dto.AgentType) ? "Company" : dto.AgentType,
            CompanyEmail = dto.Email,
            CompanyPhone = dto.Phone,
            CompanyAddress = dto.Address,
            RegNumber = dto.RegistrationNumber,
            AccountNumber = dto.AccountNumber,
            VatNumber = dto.VatNumber
        };

        private static UpdateCompanyDto ToUpdateCompany(UpdateTripAccountDto dto) => new()
        {
            CompanyId = dto.TripAccountId,
            CompanyName = dto.AgentName,
            AgentType = string.IsNullOrWhiteSpace(dto.AgentType) ? "Company" : dto.AgentType,
            CompanyEmail = dto.Email,
            CompanyPhone = dto.Phone,
            CompanyAddress = dto.Address,
            RegNumber = dto.RegistrationNumber,
            AccountNumber = dto.AccountNumber,
            VatNumber = dto.VatNumber
        };
    }
}
