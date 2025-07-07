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
                return NotFound();
            }
            return Ok(transaction);
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
                ToUserId = transactionDto.ToUserId
            };

            var createdTransaction = await _transactionService.createTransactionAsync(transaction);
            return CreatedAtAction(nameof(GetUserTransaction), new { userId = createdTransaction.UserId }, createdTransaction);
        }
    }
}
