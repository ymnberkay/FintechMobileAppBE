namespace TechMobileBE.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using TechMobileBE.Services;
    using TechMobileBE.Models;
    using System.Threading.Tasks;

    [Route("api/[controller]")]
    [ApiController]
    public class BalanceController : ControllerBase
    {
        private readonly BalanceService _balanceService;

        public BalanceController(BalanceService balanceService)
        {
            _balanceService = balanceService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserBalance(string userId)
        {
            var userBalance = await _balanceService.GetUserBalanceAsync(userId);
            if (userBalance == null)
            {
                return NotFound();
            }
            return Ok(userBalance);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserBalance([FromBody] UserBalance userBalance)
        {
            if (userBalance == null || string.IsNullOrEmpty(userBalance.UserId))
                return BadRequest(new ApiResidenceResponse<object>(false, "Invalid user balance data."));

            try
            {
                var created = await _balanceService.CreateUserBalanceAsync(userBalance);
                return Ok(new ApiResidenceResponse<UserBalance>(true, "User balance created successfully.", created));
            }
            catch (Exception ex)
            {
                return Conflict(new ApiResidenceResponse<object>(false, $"Error creating user balance: {ex.Message}"));
            }
        }
    }
}