using FinanceTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceTracker.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        private Guid GetPublicId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub");
            return Guid.Parse(claim!.Value);
        }

        [HttpGet("kpi")]
        public async Task<IActionResult> GetKpiData(
            [FromQuery] int month, [FromQuery] int year)
        {
            var result = await _dashboardService.GetKpiData(
                GetPublicId(), month, year);
            return Ok(result);
        }

        [HttpGet("trend")]
        public async Task<IActionResult> GetMonthlyTrend(
            [FromQuery] int month, [FromQuery] int year)
        {
            var result = await _dashboardService.GetMonthlyTrend(
                GetPublicId(), month, year);
            return Ok(result);
        }

        [HttpGet("income-pie")]
        public async Task<IActionResult> GetIncomePieData(
            [FromQuery] int month, [FromQuery] int year)
        {
            var result = await _dashboardService.GetIncomePieData(
                GetPublicId(), month, year);
            return Ok(result);
        }

        [HttpGet("expense-pie")]
        public async Task<IActionResult> GetExpensePieData(
            [FromQuery] int month, [FromQuery] int year)
        {
            var result = await _dashboardService.GetExpensePieData(
                GetPublicId(), month, year);
            return Ok(result);
        }
        [HttpGet("salary-trend")]
        public async Task<IActionResult> GetSalaryTrend([FromQuery] int month, [FromQuery] int year)
        {
            var result = await _dashboardService.GetSalaryTrend(
                GetPublicId(), month, year);
            return Ok(result);
        }
    }
}