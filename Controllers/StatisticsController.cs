using Microsoft.AspNetCore.Mvc;
using TechMobileBE.Services;
using TechMobileBE.Models.DTOs;

namespace TechMobileBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly StatisticsService _statisticsService;

        public StatisticsController(StatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        [HttpGet("{userId}/{month}")]
        public async Task<IActionResult> GetMonthlyStatistics(string userId, int month)
        {
            if (month < 1 || month > 12)
            {
                return BadRequest(new ApiResidenceResponse<object>(
                    success: false,
                    message: "Invalid month. Must be between 1 and 12.",
                    data: null
                ));
            }

            var statistics = await _statisticsService.GetMonthlyStatistics(userId, month);
            
            return Ok(new ApiResidenceResponse<StatisticsDto>(
                success: true,
                message: $"Statistics for month {month} fetched successfully.",
                data: statistics
            ));
        }
    }
}