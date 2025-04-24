using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace TechMobileBE.Services
{
public class PersonalInfo
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string Residence { get; set; }
    public string FullName { get; set; }
    public string UserName { get; set; }
    public string DateOfBirth { get; set; }
    public string AddressLine { get; set; }
    public string City { get; set; }
    public string PostCode { get; set; }
    public string Email { get; set; }
    public string Passcode { get; set; }

    public bool IsCompleted { get; set; }
}
}