using Microsoft.AspNetCore.Mvc;
using TechMobileBE.Services;
using TechMobileBE.Models;
using TechMobileBE.DTOs;
using System.Threading.Tasks;

namespace TechMobileBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly TransactionService _transactionService;

        public TransactionController(TransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserTransaction(string userId)
        {
            var transaction = await _transactionService.GetUserTransaction(userId);
            if (transaction == null)
            {
                return NotFound(new ApiResponse<object>(false, "Personal transactions not found."));
            }
            var response = new ApiResidenceResponse<object>(true, "Personal transactions fetched successfully.", transaction);
            return Ok(response);
        }
        

        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] Transaction transactionDto)
        {
            if (transactionDto == null)
            {
                return BadRequest("Invalid transaction data.");
            }

            var transaction = new Transaction
            {
                UserId = transactionDto.UserId,
                Type = transactionDto.Type,
                Amount = transactionDto.Amount,
                Currency = transactionDto.Currency,
                ToUserId = transactionDto.ToUserId,
                ToUserName = transactionDto.ToUserName,
                ToUserEmail = transactionDto.ToUserEmail,
            };

            var result = await _transactionService.CreateTransaction(transaction);
            if (!result.Success)
                return BadRequest(result.Message);

            return CreatedAtAction(nameof(GetUserTransaction), new { userId = result.Transaction.UserId }, result.Transaction);
        }
    }
}
