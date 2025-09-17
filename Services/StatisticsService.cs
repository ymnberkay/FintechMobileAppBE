using MongoDB.Driver;
using TechMobileBE.Models;
using TechMobileBE.Models.DTOs;

namespace TechMobileBE.Services
{
    public class StatisticsService
    {
        private readonly IMongoCollection<Transaction> _transactionCollection;
        private readonly BalanceService _balanceService;

        public StatisticsService(
            MongoDbService mongoDbService,
            BalanceService balanceService)
        {
            _transactionCollection = mongoDbService.GetCollection<Transaction>("Transactions");
            _balanceService = balanceService;
        }

        public async Task<StatisticsDto> GetMonthlyStatistics(string userId, int month)
        {
            var year = DateTime.UtcNow.Year;
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // Get current balance
            var balance = await _balanceService.GetUserBalanceAsync(userId);

            // Get all transactions for the month
            var sentTransactions = await _transactionCollection
                .Find(t => t.UserId == userId && 
                          t.CreatedAt >= startDate && 
                          t.CreatedAt <= endDate &&
                          t.Status == TransactionStatus.Completed)
                .ToListAsync();

            var receivedTransactions = await _transactionCollection
                .Find(t => t.ToUserId == userId && 
                          t.CreatedAt >= startDate && 
                          t.CreatedAt <= endDate &&
                          t.Status == TransactionStatus.Completed)
                .ToListAsync();

            // Calculate period statistics
            var periodStats = CalculatePeriodStatistics(startDate, endDate, sentTransactions, receivedTransactions);

            return new StatisticsDto
            {
                AvailableBalance = balance?.Balance ?? 0,
                TotalSent = sentTransactions.Sum(t => t.Amount),
                TotalReceived = receivedTransactions.Sum(t => t.Amount),
                PeriodStats = periodStats
            };
        }

        private List<PeriodStatistics> CalculatePeriodStatistics(
            DateTime startDate, 
            DateTime endDate,
            List<Transaction> sentTransactions,
            List<Transaction> receivedTransactions)
        {
            var periodStats = new List<PeriodStatistics>();
            var daysInMonth = (endDate - startDate).Days + 1;
            var periodLength = daysInMonth / 5;

            for (int i = 0; i < 5; i++)
            {
                var periodStart = startDate.AddDays(i * periodLength);
                var periodEnd = i == 4 ? endDate : periodStart.AddDays(periodLength - 1);

                var periodSent = sentTransactions
                    .Where(t => t.CreatedAt >= periodStart && t.CreatedAt <= periodEnd)
                    .Sum(t => t.Amount);

                var periodReceived = receivedTransactions
                    .Where(t => t.CreatedAt >= periodStart && t.CreatedAt <= periodEnd)
                    .Sum(t => t.Amount);

                var totalTransactions = sentTransactions.Count(t => t.CreatedAt >= periodStart && t.CreatedAt <= periodEnd) +
                                     receivedTransactions.Count(t => t.CreatedAt >= periodStart && t.CreatedAt <= periodEnd);

                periodStats.Add(new PeriodStatistics
                {
                    DateRange = $"{periodStart.Day}-{periodEnd.Day}",
                    SentAmount = periodSent,
                    ReceivedAmount = periodReceived,
                    TotalTransactions = totalTransactions
                });
            }

            return periodStats;
        }
    }
}