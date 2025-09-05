using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace TechMobileBE.Models
{
    public class GetMoneyRequestsResponse
    {
        [BsonElement("id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        [BsonElement("recieverUserId")]
        public string? ReceiverUserId { get; set; }

        [BsonElement("type")]
        [BsonRepresentation(BsonType.String)]
        public String Type { get; set; }

        [BsonElement("amount")]
        public decimal Amount { get; set; }

        [BsonElement("currency")]
        public string Currency { get; set; } = "USD";

        
        [BsonElement("senderUserId")]
        public string SenderUserId { get; set; } = null!;

        [BsonElement("senderUserName")]
        public string? SenderUserName { get; set; }

        [BsonElement("senderUserEmail")]
        public string? SenderUserEmail { get; set; }
    }
}
