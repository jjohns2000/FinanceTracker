using FinanceTracker.Models.Transaction;
using FinanceTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceTracker.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    [Authorize]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        private Guid GetPublicId()
        {
            var value = User.FindFirst("sub")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(value, out var guid))
                return guid;
            throw new UnauthorizedAccessException("Invalid user token.");
        }

        [HttpGet]
        public async Task<IActionResult> GetTransactions(
            [FromQuery] int month, [FromQuery] int year)
        {
            var result = await _transactionService.GetTransactions(GetPublicId(), month, year);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction(
            [FromBody] CreateTransactionRequest request)
        {
            var result = await _transactionService.CreateTransaction(GetPublicId(), request);
            return Ok(new { publicId = result });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateTransaction(
            [FromBody] UpdateTransactionRequest request)
        {
            await _transactionService.UpdateTransaction(GetPublicId(), request);
            return Ok();
        }

        [HttpDelete("{publicId:guid}")]
        public async Task<IActionResult> DeleteTransaction(Guid publicId)
        {
            await _transactionService.DeleteTransaction(GetPublicId(), publicId);
            return Ok();
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> CreateTransfer(
            [FromBody] CreateTransferRequest request)
        {
            var result = await _transactionService.CreateTransfer(GetPublicId(), request);
            return Ok(new { publicId = result });
        }

        [HttpDelete("transfer/{publicId:guid}")]
        public async Task<IActionResult> DeleteTransfer(Guid publicId)
        {
            await _transactionService.DeleteTransfer(GetPublicId(), publicId);
            return Ok();
        }

        [HttpGet("payment-methods")]
        public async Task<IActionResult> GetPaymentMethods()
        {
            var result = await _transactionService.GetPaymentMethods();
            return Ok(result);
        }
    }
}