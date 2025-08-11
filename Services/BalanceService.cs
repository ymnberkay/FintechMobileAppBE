using MongoDB.Driver;
using TechMobileBE.Models;


namespace TechMobileBE.Services
{
    public class BalanceService
    {
        private readonly IMongoCollection<UserBalance> _userBalanceCollection;
        private readonly IMongoCollection<User> _userCollection;

        public BalanceService(MongoDbService mongoDbService)
        {
            _userBalanceCollection = mongoDbService.GetCollection<UserBalance>("UserBalances");
            _userCollection = mongoDbService.GetCollection<User>("Auth");
        }

        public async Task<UserBalance> GetUserBalanceAsync(string userId)
        {
            return await _userBalanceCollection
            .Find(balance => balance.UserId == userId)
            .FirstOrDefaultAsync();
        }

        public async Task UpdateUserBalanceAsync(string userId, decimal amount)
        {
            var filter = Builders<UserBalance>.Filter.Eq(b => b.UserId, userId);
            var update = Builders<UserBalance>.Update.Inc(b => b.Balance, amount);

            await _userBalanceCollection.UpdateOneAsync(filter, update);
        }

        public async Task<UserBalance> CreateUserBalanceAsync(UserBalance userBalance)
        {
            // Kullanıcı gerçekten var mı kontrolü
            var userExists = await _userCollection
                .Find(user => user.Id == userBalance.UserId)
                .AnyAsync();

            if (!userExists)
                throw new Exception("User not found.");

            var existing = await GetUserBalanceAsync(userBalance.UserId);
            if (existing != null)
                throw new Exception("User balance already exists.");

            userBalance.LastUpdated = DateTime.UtcNow;
            await _userBalanceCollection.InsertOneAsync(userBalance);
            return userBalance;
        }
        
        public async Task<UserBalance> IncreaseUserBalanceAsync(string userId, decimal amount)
        {
            var userBalance = await GetUserBalanceAsync(userId);
            if (userBalance == null)
            {
                userBalance = new UserBalance
                {
                    UserId = userId,
                    Balance = amount,
                    Currency = "USD",
                    LastUpdated = DateTime.UtcNow
                };
                await _userBalanceCollection.InsertOneAsync(userBalance);
            }
            else
            {
                await UpdateUserBalanceAsync(userId, amount);
                userBalance.Balance += amount;
                userBalance.LastUpdated = DateTime.UtcNow;
            }
            return userBalance;
        }
        public async Task<UserBalance> DecreaseUserBalanceAsync(string userId, decimal amount)
        {
            var userBalance = await GetUserBalanceAsync(userId);
            if (userBalance == null)
            {
                throw new Exception("User balance not found.");
            }

            if (userBalance.Balance < amount)
            {
                throw new Exception("Insufficient balance.");
            }

            await UpdateUserBalanceAsync(userId, -amount);
            userBalance.Balance -= amount;
            userBalance.LastUpdated = DateTime.UtcNow;

            return userBalance;
        }

        

    }
}