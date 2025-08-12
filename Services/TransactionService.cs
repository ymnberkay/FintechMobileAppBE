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
        private readonly BalanceService _balanceService;
        private readonly PersonalInfoService _personalInfoService; // yeni ekle

        public TransactionService(
            MongoDbService mongoDbService,
            BalanceService balanceService,
            PersonalInfoService personalInfoService // yeni ekle
        )
        {
            // Get the correct collections
            _transactionCollection = mongoDbService.GetCollection<Transaction>("Transactions");
            _balanceService = balanceService;
            _personalInfoService = personalInfoService; // yeni ekle
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

        
    }
        
        
    
}