using FinanceTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/nav")]
    public class NavController : ControllerBase
    {
        private readonly INavService _navService;

        public NavController(INavService navService)
        {
            _navService = navService;
        }

        [HttpGet]
        public async Task<IActionResult> GetNavItems()
        {
            var result = await _navService.GetNavItems();
            return Ok(result);
        }
    }
}