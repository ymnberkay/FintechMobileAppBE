using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class UserBalance
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }

    [BsonElement("userId")]
    public string UserId { get; set; }

    [BsonElement("balance")]
    public decimal Balance { get; set; }

    [BsonElement("currency")]
    public string Currency { get; set; } = "USD";

    
    [BsonElement("lastUpdated")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
} 