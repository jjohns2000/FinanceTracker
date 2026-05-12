using FinanceTracker.Models.Expense;
using FinanceTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceTracker.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/expense-types")]
    public class ExpenseTypeController : ControllerBase
    {
        private readonly IExpenseTypeService _expenseTypeService;

        public ExpenseTypeController(IExpenseTypeService expenseTypeService)
        {
            _expenseTypeService = expenseTypeService;
        }

        private Guid GetPublicId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub");
            return Guid.Parse(claim!.Value);
        }

        [HttpGet]
        public async Task<IActionResult> GetExpenseTypes()
        {
            var result = await _expenseTypeService.GetExpenseTypes(GetPublicId());
            return Ok(result);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveExpenseTypes(
            [FromQuery] int month, [FromQuery] int year)
        {
            var result = await _expenseTypeService.GetActiveExpenseTypes(
                GetPublicId(), month, year);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateExpenseType(
            [FromBody] CreateExpenseTypeRequest request)
        {
            var result = await _expenseTypeService.CreateExpenseType(
                GetPublicId(), request);
            return Ok(new { id = result });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateExpenseType(
            [FromBody] UpdateExpenseTypeRequest request)
        {
            await _expenseTypeService.UpdateExpenseType(request);
            return Ok();
        }

        [HttpDelete("{publicId}")]
        public async Task<IActionResult> DeleteExpenseType(Guid publicId)
        {
            await _expenseTypeService.DeleteExpenseType(publicId);
            return Ok();
        }

        [HttpGet("entries")]
        public async Task<IActionResult> GetMonthlyExpenseEntries(
            [FromQuery] int month, [FromQuery] int year)
        {
            var result = await _expenseTypeService.GetMonthlyExpenseEntries(
                GetPublicId(), month, year);
            return Ok(result);
        }

        [HttpPost("entries")]
        public async Task<IActionResult> UpsertMonthlyExpenseEntry(
            [FromBody] UpsertMonthlyExpenseEntryRequest request)
        {
            var result = await _expenseTypeService.UpsertMonthlyExpenseEntry(
                GetPublicId(), request);
            return Ok(new { publicId = result });
        }

        [HttpDelete("entries/{publicId}")]
        public async Task<IActionResult> DeleteMonthlyExpenseEntry(Guid publicId)
        {
            await _expenseTypeService.DeleteMonthlyExpenseEntry(publicId);
            return Ok();
        }
    }
}