using FinanceTracker.Models.Transaction;
using FinanceTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceTracker.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/transactions")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        private Guid GetPublicId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub");
            return Guid.Parse(claim!.Value);
        }

        [HttpGet("payment-methods")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPaymentMethods()
        {
            var result = await _transactionService.GetPaymentMethods();
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetTransactions(
            [FromQuery] int month, [FromQuery] int year)
        {
            var result = await _transactionService.GetTransactions(
                GetPublicId(), month, year);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction(
            [FromBody] CreateTransactionRequest request)
        {
            var result = await _transactionService.CreateTransaction(
                GetPublicId(), request);
            return Ok(new { id = result });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateTransaction(
            [FromBody] UpdateTransactionRequest request)
        {
            await _transactionService.UpdateTransaction(request);
            return Ok();
        }

        [HttpDelete("{publicId}")]
        public async Task<IActionResult> DeleteTransaction(Guid publicId)
        {
            await _transactionService.DeleteTransaction(publicId);
            return Ok();
        }
    }
}