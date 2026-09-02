using FinanceTracker.Models.Statement;
using FinanceTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceTracker.Controllers
{
    [ApiController]
    [Route("api/statements")]
    [Authorize]
    public class StatementController : ControllerBase
    {
        private readonly IStatementService _statementService;

        public StatementController(IStatementService statementService)
        {
            _statementService = statementService;
        }

        private Guid GetPublicId()
        {
            var value = User.FindFirst("sub")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(value, out var guid))
                return guid;
            throw new UnauthorizedAccessException("Invalid user token.");
        }

        [HttpPost("extract")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> ExtractStatement(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var allowed = new[] { ".pdf", ".csv", ".txt", ".png", ".jpg", ".jpeg" };
            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!allowed.Contains(ext))
                return BadRequest("Unsupported file type. Upload a PDF, CSV, or image.");

            var result = await _statementService.ExtractStatement(GetPublicId(), file);
            return Ok(result);
        }

        [HttpPost("check-duplicates")]
        public async Task<IActionResult> CheckDuplicates([FromBody] DuplicateCheckRequest request)
        {
            var duplicates = await _statementService.CheckDuplicates(GetPublicId(), request);
            return Ok(duplicates);
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportStatement([FromBody] StatementImportRequest request)
        {
            var result = await _statementService.ImportStatement(GetPublicId(), request);
            return Ok(result);
        }
    }
}