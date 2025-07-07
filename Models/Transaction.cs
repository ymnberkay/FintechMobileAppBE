using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace TechMobileBE.Models
{
    public class Transaction
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("userId")]
        public string UserId { get; set; } = null!;

        [BsonElement("type")]
        public string Type { get; set; } = null!; // deposit, withdrawal, transfer

        [BsonElement("amount")]
        public decimal Amount { get; set; }

        [BsonElement("currency")]
        public string Currency { get; set; } = "USD";

        [BsonElement("status")]
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

        [BsonElement("toUserId")]
        public string? ToUserId { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }

    public enum TransactionType
    {
        Deposit,
        Withdraw,
        Transfer
    }

    public enum TransactionStatus
{
    Pending,
    Completed,
    Failed,
    Cancelled
}
}
