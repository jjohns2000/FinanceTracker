using FinanceTracker.Models.Employment;
using FinanceTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceTracker.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/employment")]
    public class EmploymentController : ControllerBase
    {
        private readonly IEmploymentService _employmentService;

        public EmploymentController(IEmploymentService employmentService)
        {
            _employmentService = employmentService;
        }

        private Guid GetPublicId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub");
            return Guid.Parse(claim!.Value);
        }

        [HttpGet("types")]
        public async Task<IActionResult> GetEmploymentTypes()
        {
            var result = await _employmentService.GetEmploymentTypes();
            return Ok(result);
        }

        [HttpGet("frequencies")]
        public async Task<IActionResult> GetPayFrequencies()
        {
            var result = await _employmentService.GetPayFrequencies();
            return Ok(result);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveEmployments(
            [FromQuery] int month, [FromQuery] int year)
        {
            var result = await _employmentService.GetActiveEmployments(
                GetPublicId(), month, year);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployment(
            [FromBody] CreateEmploymentRequest request)
        {
            var result = await _employmentService.CreateEmployment(
                GetPublicId(), request);
            return Ok(new { id = result });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateEmployment(
            [FromBody] UpdateEmploymentRequest request)
        {
            await _employmentService.UpdateEmployment(request);
            return Ok();
        }

        [HttpDelete("{publicId}")]
        public async Task<IActionResult> DeleteEmployment(Guid publicId)
        {
            await _employmentService.DeleteEmployment(publicId);
            return Ok();
        }

        [HttpGet("salaries")]
        public async Task<IActionResult> GetMonthlySalaries(
            [FromQuery] int month, [FromQuery] int year)
        {
            var result = await _employmentService.GetMonthlySalaries(
                GetPublicId(), month, year);
            return Ok(result);
        }

        [HttpPost("salaries")]
        public async Task<IActionResult> UpsertMonthlySalary(
            [FromBody] UpsertMonthlySalaryRequest request)
        {
            var result = await _employmentService.UpsertMonthlySalary(
                GetPublicId(), request);
            return Ok(new { publicId = result });
        }

        [HttpDelete("salaries/{publicId}")]
        public async Task<IActionResult> DeleteMonthlySalary(Guid publicId)
        {
            await _employmentService.DeleteMonthlySalary(publicId);
            return Ok();
        }
    }
}