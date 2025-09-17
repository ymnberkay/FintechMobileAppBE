using Microsoft.AspNetCore.Mvc;
using TechMobileBE.Services;
using TechMobileBE.Models;
using TechMobileBE.DTOs;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

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
            var transactions = await _transactionService.GetUserTransaction(userId);

            if (transactions == null || !transactions.Any())
            {
                return NotFound(new ApiResponse<object>(false, "Personal transactions not found."));
            }

            var response = new ApiGetTransactionResponse<object>(
                success: true,
                message: "Personal transactions fetched successfully.",
                data: transactions
            );

            return Ok(response);
        }


        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] Transaction transactionDto)
        {
            if (transactionDto == null)
            {
                return BadRequest(new ApiGetTransactionResponse<object>(
                    success: false,
                    message: "Invalid transaction data.",
                    data: null
                ));
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
            {
                return BadRequest(new ApiGetTransactionResponse<Transaction>(
                    success: false,
                    message: result.Message,
                    data: null
                ));
            }

            var response = new ApiGetTransactionResponse<Transaction>(
                success: true,
                message: "Transaction created successfully.",
                data: result.Transaction
            );

            return Created($"api/transaction/{result.Transaction.Id}", response);
        }


        [HttpGet("getRequests/{userID}")]
        public async Task<IActionResult> GetUserRequests(string userId)
        {
            var result = await _transactionService.GetUserRequestsAsync(userId);
            if (result == null || !result.Any())
            {
                return NotFound(new ApiResponse<object>(false, "User Requests not found."));
            }

            var response = new ApiGetTransactionResponse<List<GetMoneyRequestsResponse>>(
                success: true,
                message: "User requests fetched successfully.",
                data: result
            );
            return Ok(response);

        }

        [HttpPost("RequestMoney")]
        public async Task<IActionResult> CreateMoneyRequest([FromBody] MoneyRequest moneyRequestDto)
        {
            if (moneyRequestDto == null)
            {
                return BadRequest(new ApiGetTransactionResponse<object>(
                    success: false,
                    message: "Invalid money request data.",
                    data: null
                ));
            }

            var moneyRequest = new MoneyRequest
            {
                UserId = moneyRequestDto.UserId,
                Type = moneyRequestDto.Type,
                Amount = moneyRequestDto.Amount,
                Currency = moneyRequestDto.Currency,
                ToUserId = moneyRequestDto.ToUserId,
                ToUserName = moneyRequestDto.ToUserName,
                ToUserEmail = moneyRequestDto.ToUserEmail,
            };

            var result = await _transactionService.RequestTransaction(moneyRequest);
            if (!result.Success)
            {
                return BadRequest(new ApiGetTransactionResponse<MoneyRequest>(
                    success: false,
                    message: result.Message,
                    data: null
                ));
            }

            var response = new ApiGetTransactionResponse<MoneyRequest>(
                success: true,
                message: "Money request created successfully.",
                data: result.Transaction
            );

            return Created($"api/transaction/request/{result.Transaction.Id}", response);
        }
        
        [HttpPut("handle-request")]
        public async Task<IActionResult> HandleMoneyRequest([FromBody] HandleMoneyRequestDto request)
        {
            if (string.IsNullOrEmpty(request.RequestId))
            {
                return BadRequest(new ApiGetTransactionResponse<object>(
                    success: false,
                    message: "Request ID cannot be empty.",
                    data: null
                ));
            }

            try
            {
                Console.WriteLine($"Processing request ID: {request.RequestId}, Approve: {request.Approve}");
                
                var result = await _transactionService.HandleMoneyRequest(request.RequestId, request.Approve);

                Console.WriteLine($"Service result - Success: {result?.Success}, Message: {result?.Message}");

                if (!result.Success)
                {
                    return BadRequest(new ApiGetTransactionResponse<MoneyRequest>(
                        success: false,
                        message: result.Message,
                        data: result.Transaction
                    ));
                }

                return Ok(new ApiGetTransactionResponse<MoneyRequest>(
                    success: true,
                    message: request.Approve ? "Money request approved and transferred successfully." 
                                           : "Money request rejected successfully.",
                    data: result.Transaction
                ));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in HandleMoneyRequest: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                return StatusCode(500, new ApiGetTransactionResponse<object>(
                    success: false,
                    message: $"An error occurred: {ex.Message}",
                    data: null
                ));
            }
        }
    }
}
