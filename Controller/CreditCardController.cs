using FinanceTracker.Models.CreditCard;
using FinanceTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceTracker.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/credit-cards")]
    public class CreditCardController : ControllerBase
    {
        private readonly ICreditCardService _creditCardService;

        public CreditCardController(ICreditCardService creditCardService)
        {
            _creditCardService = creditCardService;
        }

        private Guid GetPublicId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub");
            return Guid.Parse(claim!.Value);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserCreditCards()
        {
            var result = await _creditCardService.GetUserCreditCards(GetPublicId());
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCreditCard(
            [FromBody] CreateCreditCardRequest request)
        {
            var result = await _creditCardService.CreateCreditCard(GetPublicId(), request);
            return Ok(new { id = result });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCreditCard(
            [FromBody] UpdateCreditCardRequest request)
        {
            await _creditCardService.UpdateCreditCard(request);
            return Ok();
        }

        [HttpDelete("{publicId}")]
        public async Task<IActionResult> DeleteCreditCard(Guid publicId)
        {
            await _creditCardService.DeleteCreditCard(publicId);
            return Ok();
        }

        [HttpGet("monthly")]
        public async Task<IActionResult> GetMonthlyCreditCardSummary(
            [FromQuery] int month, [FromQuery] int year)
        {
            var result = await _creditCardService.GetMonthlyCreditCardSummary(
                GetPublicId(), month, year);
            return Ok(result);
        }

        [HttpPost("monthly")]
        public async Task<IActionResult> UpsertMonthlyCreditCardSummary(
            [FromBody] UpsertMonthlyCreditCardRequest request)
        {
            var result = await _creditCardService.UpsertMonthlyCreditCardSummary(
                GetPublicId(), request);
            return Ok(new { publicId = result });
        }
    }
}