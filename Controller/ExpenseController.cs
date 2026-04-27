using FinanceTracker.Models.Income;
using FinanceTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceTracker.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/expense")]
    public class ExpenseController : ControllerBase
    {
        private readonly IIncomeService _incomeService;

        public ExpenseController(IIncomeService incomeService)
        {
            _incomeService = incomeService;
        }

        private Guid GetPublicId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub");
            return Guid.Parse(claim!.Value);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetMonthlyAccountSummary(
            [FromQuery] int month, [FromQuery] int year)
        {
            var result = await _incomeService.GetMonthlyAccountSummary(
                GetPublicId(), month, year);
            return Ok(result);
        }

        [HttpGet("aggregate")]
        public async Task<IActionResult> GetMonthlyAggregate(
            [FromQuery] int month, [FromQuery] int year)
        {
            var result = await _incomeService.GetMonthlyAggregate(
                GetPublicId(), month, year);
            return Ok(result);
        }

        [HttpPost("upsert-withdrawal")]
        public async Task<IActionResult> UpsertWithdrawal(
            [FromBody] UpsertWithdrawalRequest request)
        {
            var result = await _incomeService.UpsertWithdrawal(
                GetPublicId(), request);
            return Ok(new { publicId = result });
        }
    }
}