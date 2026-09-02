using FinanceTracker.Models.CheckIn;
using FinanceTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceTracker.Controllers
{
    [ApiController]
    [Route("api/checkin")]
    [Authorize]
    public class CheckInController : ControllerBase
    {
        private readonly ICheckInService _checkInService;

        public CheckInController(ICheckInService checkInService)
        {
            _checkInService = checkInService;
        }

        private Guid GetPublicId()
        {
            var value = User.FindFirst("sub")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(value, out var guid))
                return guid;

            throw new UnauthorizedAccessException("Invalid user token.");
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetCheckInStatus()
        {
            var result = await _checkInService.GetCheckInStatus(GetPublicId());
            return Ok(result);
        }

        [HttpPost("save")]
        public async Task<IActionResult> SaveAnswer([FromBody] CheckInSaveRequest request)
        {
            await _checkInService.SaveAnswer(GetPublicId(), request);
            return Ok();
        }
    }
}