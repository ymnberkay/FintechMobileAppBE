using System;
using System.Collections.Generic;

namespace TechMobileBE.Models.DTOs
{
    public class StatisticsDto
    {
        public decimal AvailableBalance { get; set; }
        public decimal TotalSent { get; set; }
        public decimal TotalReceived { get; set; }
        public List<PeriodStatistics> PeriodStats { get; set; } = new();
    }

    public class PeriodStatistics
    {
        public string DateRange { get; set; } = string.Empty;
        public decimal SentAmount { get; set; }
        public decimal ReceivedAmount { get; set; }
        public decimal TotalTransactions { get; set; }
    }
}