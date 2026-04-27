using FinanceTracker.Models.Settings;
using FinanceTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceTracker.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/settings")]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingsService _settingsService;

        public SettingsController(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        private Guid GetPublicId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub");
            return Guid.Parse(claim!.Value);
        }

        [HttpGet("banks")]
        public async Task<IActionResult> GetBanks()
        {
            var result = await _settingsService.GetBanks();
            return Ok(result);
        }

        [HttpGet("account-types")]
        public async Task<IActionResult> GetAccountTypes()
        {
            var result = await _settingsService.GetAccountTypes();
            return Ok(result);
        }

        [HttpGet("accounts")]
        public async Task<IActionResult> GetUserAccounts()
        {
            var result = await _settingsService.GetUserAccounts(GetPublicId());
            return Ok(result);
        }

        [HttpPost("accounts")]
        public async Task<IActionResult> CreateUserAccount([FromBody] CreateUserAccountRequest request)
        {
            var result = await _settingsService.CreateUserAccount(GetPublicId(), request);
            return Ok(new { id = result });
        }

        [HttpPut("accounts")]
        public async Task<IActionResult> UpdateUserAccount([FromBody] UpdateUserAccountRequest request)
        {
            await _settingsService.UpdateUserAccount(request);
            return Ok();
        }

        [HttpDelete("accounts/{publicId}")]
        public async Task<IActionResult> DeleteUserAccount(Guid publicId)
        {
            await _settingsService.DeleteUserAccount(publicId);
            return Ok();
        }
    }
}