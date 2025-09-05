using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace TechMobileBE.Models
{
    public class MoneyRequest
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("userId")]
        public string UserId { get; set; } = null!;

        [BsonElement("type")]
        [BsonRepresentation(BsonType.String)]
        public String Type { get; set; }

        [BsonElement("amount")]
        public decimal Amount { get; set; }

        [BsonElement("currency")]
        public string Currency { get; set; } = "USD";

        [BsonElement("status")]
        [BsonRepresentation(BsonType.String)]
        public RequestTransactionStatus Status { get; set; } = RequestTransactionStatus.Pending;

        [BsonElement("toUserId")]
        public string? ToUserId { get; set; }

        [BsonElement("toUserName")]
        public string? ToUserName { get; set; }

        [BsonElement("toUserEmail")]
        public string? ToUserEmail { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime? UpdatedAt { get; set; }
    }

   public enum MoneyRequestStatus
    {
        Pending,
        Approved,
        Rejected,
        Cancelled,
        Expired
    }

    public enum RequestTransactionType
    {
        Personal,
        Business,
        Payment,
        MoneyRequest,
        RequestApproved,
        RequestRejected
    }

    public enum RequestTransactionStatus
    {
        Pending,
        Completed,
        Failed,
        Cancelled
    }
}