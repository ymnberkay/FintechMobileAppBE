using MongoDB.Driver;
using TechMobileBE.Models;
using Microsoft.AspNetCore.SignalR;
using TechMobileBE.Hubs;



namespace TechMobileBE.Services
{

    public class TransactionService
    {
        private readonly IMongoCollection<Transaction> _transactionCollection;
        private readonly BalanceService _balanceService;

        public TransactionService(MongoDbService mongoDbService, BalanceService balanceService)
        {
             // Get the correct collections
            _transactionCollection = mongoDbService.GetCollection<Transaction>("Transactions");
            _balanceService = balanceService;
        }
        public async Task<Transaction> GetUserTransaction(string userId) =>
            await _transactionCollection.Find(t => t.UserId == userId)
                .SortByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync();

        public async Task<Transaction> createTransactionAsync(Transaction transaction)
        {
            var type = transaction.Type.ToLower();
            if (type is not ("deposit" or "withdraw" or "transfer"))
            {
                throw new ArgumentException("Invalid transaction type.");
            }
            transaction.CreatedAt = DateTime.UtcNow;
            transaction.Status = TransactionStatus.Pending;

            if (type == "deposit")
            {
                await _balanceService.IncreaseUserBalanceAsync(transaction.UserId, transaction.Amount);
                transaction.Status = TransactionStatus.Completed;
            }
            else if (type == "withdraw")
            {
                var userBalance = await _balanceService.GetUserBalanceAsync(transaction.UserId);
                if (userBalance == null || userBalance.Balance < transaction.Amount)
                {
                    throw new Exception("Insufficient balance for withdrawal.");
                    transaction.Status = TransactionStatus.Failed;
                }
                await _balanceService.DecreaseUserBalanceAsync(transaction.UserId, transaction.Amount);
            }
            else if (type == "transfer")
            {
                if (transaction.ToUserId == null)
                {
                    throw new ArgumentException("ToUserId must be provided for transfer transactions.");
                }
                var senderBalance = await _balanceService.GetUserBalanceAsync(transaction.UserId);
                if (senderBalance == null || senderBalance.Balance < transaction.Amount)
                {
                    throw new Exception("Insufficient balance for transfer.");
                    transaction.Status = TransactionStatus.Failed;
                }
                else
                {
                    await _balanceService.DecreaseUserBalanceAsync(transaction.UserId, transaction.Amount);
                    await _balanceService.IncreaseUserBalanceAsync(transaction.ToUserId, transaction.Amount);
                    transaction.Status = TransactionStatus.Completed;
                }
            }
            await _transactionCollection.InsertOneAsync(transaction);
            return transaction;
        }

    }
        
        
    
}