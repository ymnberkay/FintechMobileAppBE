using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TechMobileBE.Models
{
    public class User
    {
        [BsonId]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("phoneNumber")]
        public string PhoneNumber { get; set; }

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; }
        
        [BsonElement("passcodeHash")]
        public string? Passcode { get; set; }

        [BsonElement("createDate")]
        public DateTime CreateDate { get; set; }
    }
}