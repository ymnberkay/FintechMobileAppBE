using MongoDB.Driver;
using MongoDB.Bson;
using TechMobileBE.Models;
using Microsoft.AspNetCore.SignalR;
using TechMobileBE.Hubs;


namespace TechMobileBE.Services
{

    public class TransactionService
    {
        private readonly IMongoCollection<Transaction> _transactionCollection;
        private readonly IMongoCollection<MoneyRequest> _moneyRequestCollection;
        private readonly BalanceService _balanceService;
        private readonly PersonalInfoService _personalInfoService;

        public TransactionService(
            MongoDbService mongoDbService,
            BalanceService balanceService,
            PersonalInfoService personalInfoService
        )
        {
            // Get the correct collections
            _transactionCollection = mongoDbService.GetCollection<Transaction>("Transactions");
            _moneyRequestCollection = mongoDbService.GetCollection<MoneyRequest>("TransactionRequests");
            _balanceService = balanceService;
            _personalInfoService = personalInfoService;
        }
        public async Task<List<Transaction>> GetUserTransaction(string userId) =>
            await _transactionCollection.Find(t => t.UserId == userId)
                .SortByDescending(t => t.CreatedAt)
                .Limit(5)
                .ToListAsync();

        public async Task<TransactionResult> CreateTransaction(Transaction transaction)
        {
            if (transaction.Amount <= 0)
            {
                return new TransactionResult
                {
                    Success = false,
                    Message = "Transaction amount must be greater than zero.",
                    Transaction = transaction
                };
            }

            if (string.IsNullOrEmpty(transaction.ToUserId) ||
                string.IsNullOrEmpty(transaction.ToUserName) ||
                string.IsNullOrEmpty(transaction.ToUserEmail))
            {
                return new TransactionResult
                {
                    Success = false,
                    Message = "ToUserId, ToUserName, and ToUserEmail must be provided.",
                    Transaction = transaction
                };
            }

            // 1. Gönderici user var mı? (balance kontrolü)
            var senderBalance = await _balanceService.GetUserBalanceAsync(transaction.UserId);
            if (senderBalance == null)
            {
                return new TransactionResult
                {
                    Success = false,
                    Message = "Sender user not found or no balance record.",
                    Transaction = transaction
                };
            }

            // 2. Alıcı user var mı? (balance kontrolü)
            var receiverBalance = await _balanceService.GetUserBalanceAsync(transaction.ToUserId);
            if (receiverBalance == null)
            {
                return new TransactionResult
                {
                    Success = false,
                    Message = "Receiver user not found or no balance record.",
                    Transaction = transaction
                };
            }

            // 3. Alıcı bilgileri PersonalInfoService ile kontrol et
            var receiverPersonalInfo = await _personalInfoService.GetPersonalInfoAsync(transaction.ToUserId);
            if (receiverPersonalInfo == null)
            {
                return new TransactionResult
                {
                    Success = false,
                    Message = "Receiver personal info not found.",
                    Transaction = transaction
                };
            }
            if (!string.Equals(receiverPersonalInfo.UserName, transaction.ToUserName, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(receiverPersonalInfo.Email, transaction.ToUserEmail, StringComparison.OrdinalIgnoreCase))
            {
                return new TransactionResult
                {
                    Success = false,
                    Message = "Receiver information does not match with userId.",
                    Transaction = transaction
                };
            }

            // 4. Göndericinin bakiyesi yeterli mi?
            if (senderBalance.Balance < transaction.Amount)
            {
                return new TransactionResult
                {
                    Success = false,
                    Message = "Insufficient balance.",
                    Transaction = transaction
                };
            }

            // Transaction işlemleri öncesi Id ataması
            if (string.IsNullOrEmpty(transaction.Id))
            {
                transaction.Id = ObjectId.GenerateNewId().ToString();
            }

            transaction.Currency ??= "USD";
            transaction.Status = TransactionStatus.Pending;
            transaction.CreatedAt = DateTime.UtcNow;

            // Insert the transaction into the database
            await _transactionCollection.InsertOneAsync(transaction);

            // Update the sender's balance (subtract)
            await _balanceService.DecreaseUserBalanceAsync(transaction.UserId, transaction.Amount);

            // Update the receiver's balance (add)
            await _balanceService.IncreaseUserBalanceAsync(transaction.ToUserId, transaction.Amount);

            // Update transaction status to Completed
            var update = Builders<Transaction>.Update.Set(t => t.Status, TransactionStatus.Completed);
            await _transactionCollection.UpdateOneAsync(
                t => t.Id == transaction.Id,
                update
            );

            // Update the transaction object in memory as well
            transaction.Status = TransactionStatus.Completed;

            return new TransactionResult
            {
                Success = true,
                Message = "Transaction created successfully.",
                Transaction = transaction
            };
        }
        public async Task<RequestTransactionResult> RequestTransaction(MoneyRequest request)
        {
            if (request.Amount <= 0)
            {
                return new RequestTransactionResult
                {
                    Success = false,
                    Message = "Request amount must be greater than zero.",
                    Transaction = request
                };
            }

            // Check if target user exists
            var requestedFromUser = await _personalInfoService.GetPersonalInfoAsync(request.ToUserId);
            if (requestedFromUser == null)
            {
                return new RequestTransactionResult
                {
                    Success = false,
                    Message = "Requested user not found.",
                    Transaction = request
                };
            }

            request.Id = ObjectId.GenerateNewId().ToString();
            request.Status = (RequestTransactionStatus)MoneyRequestStatus.Pending;
            request.CreatedAt = DateTime.UtcNow;
            request.Currency ??= "USD";

            await _moneyRequestCollection.InsertOneAsync(request);

            return new RequestTransactionResult
            {
                Success = true,
                Message = "Money request sent successfully.",
                Transaction = request
            };
        }

        public async Task<RequestTransactionResult> HandleMoneyRequest(string moneyRequestId, bool isApproved)
        {
            try
            {
                var request = await _moneyRequestCollection
                    .Find(r => r.Id == moneyRequestId)
                    .FirstOrDefaultAsync();

                if (request == null)
                {
                    return new RequestTransactionResult
                    {
                        Success = false,
                        Message = "Money request not found.",
                        Transaction = null
                    };
                }

                if (isApproved)
                {
                    var userBalance = await _balanceService.GetUserBalanceAsync(request.ToUserId);
                    if (userBalance == null || userBalance.Balance < request.Amount)
                    {
                        return new RequestTransactionResult
                        {
                            Success = false,
                            Message = "Insufficient balance to approve request.",
                            Transaction = request
                        };
                    }

                    // Create transfer transaction
                    var receiverInfo = await _personalInfoService.GetPersonalInfoAsync(request.UserId);
                    if (receiverInfo == null)
                    {
                        return new RequestTransactionResult
                        {
                            Success = false,
                            Message = "Receiver personal info not found.",
                            Transaction = request
                        };
                    }

                    // Create transfer transaction with correct user information
                    var transaction = new Transaction
                    {
                        UserId = request.ToUserId,        // From approver
                        ToUserId = request.UserId,        // To original requester
                        Amount = request.Amount,
                        ToUserEmail = receiverInfo.Email, // Use actual receiver email
                        ToUserName = receiverInfo.UserName, // Use actual receiver username
                        Currency = request.Currency,
                        Type = TransactionType.Payment.ToString(),
                        Status = TransactionStatus.Pending  // Let CreateTransaction handle the status
                    };

                    var transferResult = await CreateTransaction(transaction);
                    if (!transferResult.Success)
                    {
                        return new RequestTransactionResult
                        {
                            Success = false,
                            Message = transferResult.Message,
                            Transaction = request
                        };
                    }

                    // Update only mutable fields
                    var update = Builders<MoneyRequest>.Update
                        .Set(r => r.Status, (RequestTransactionStatus)MoneyRequestStatus.Approved)
                        .Set(r => r.UpdatedAt, DateTime.UtcNow);

                    await _moneyRequestCollection.UpdateOneAsync(
                        r => r.Id == moneyRequestId,
                        update
                    );

                    request.Status = (RequestTransactionStatus)MoneyRequestStatus.Approved;
                    request.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Update for rejection
                    var update = Builders<MoneyRequest>.Update
                        .Set(r => r.Status, (RequestTransactionStatus)MoneyRequestStatus.Rejected)
                        .Set(r => r.UpdatedAt, DateTime.UtcNow);

                    await _moneyRequestCollection.UpdateOneAsync(
                        r => r.Id == moneyRequestId,
                        update
                    );

                    request.Status = (RequestTransactionStatus)MoneyRequestStatus.Rejected;
                    request.UpdatedAt = DateTime.UtcNow;
                }

                return new RequestTransactionResult
                {
                    Success = true,
                    Message = isApproved ? "Money request approved and transferred successfully." 
                                        : "Money request rejected successfully.",
                    Transaction = request
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in HandleMoneyRequest service: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<List<GetMoneyRequestsResponse>> GetUserRequestsAsync(string userId)
        {
            var filter = Builders<MoneyRequest>.Filter.Eq(r => r.ToUserId, userId) &
                         Builders<MoneyRequest>.Filter.Eq(r => r.Type, "MoneyRequest") &
                         Builders<MoneyRequest>.Filter.Eq(r => r.Status, (RequestTransactionStatus)MoneyRequestStatus.Pending);

            var requests = await _moneyRequestCollection.Find(filter).ToListAsync();
            var result = new List<GetMoneyRequestsResponse>();

            foreach (var req in requests)
            {

                var personalInfo = await _personalInfoService.GetPersonalInfoAsync(req.UserId);

                result.Add(new GetMoneyRequestsResponse
                {
                    Id = req.Id!,
                    ReceiverUserId = req.ToUserId,
                    Type = req.Type,
                    Amount = req.Amount,
                    Currency = req.Currency,
                    SenderUserId = req.UserId,
                    SenderUserName = personalInfo?.UserName,
                    SenderUserEmail = personalInfo?.Email
                });
            
        }
            return result;
        }


    }



}