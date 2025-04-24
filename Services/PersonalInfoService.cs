using MongoDB.Driver;

namespace TechMobileBE.Services
{
    public class PersonalInfoService
    {
        private readonly IMongoCollection<PersonalInfo> _collection;

        public PersonalInfoService(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("MongoDb"));
            var db = client.GetDatabase("TechMobileDB");
            _collection = db.GetCollection<PersonalInfo>("PersonalInfos");
        }

        public async Task<string> CreateResidenceAsync(Step1ResidenceDto dto)
        {
            var info = new PersonalInfo
            {
                Residence = dto.Residence
            };

            await _collection.InsertOneAsync(info);
            return info.Id!;
        }

        public async Task<bool> UpdateStep2Async(Step2NameDto dto)
        {
            var update = Builders<PersonalInfo>.Update
                .Set(x => x.FullName, dto.FullName)
                .Set(x => x.UserName, dto.UserName)
                .Set(x => x.DateOfBirth, dto.DateOfBirth); // Format: "DD-MM-YYYY"

            var result = await _collection.UpdateOneAsync(x => x.Id == dto.Id, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateStep3Async(Step3AddressDto dto)
        {
            var update = Builders<PersonalInfo>.Update
                .Set(x => x.AddressLine, dto.AddressLine)
                .Set(x => x.City, dto.City)
                .Set(x => x.PostCode, dto.PostCode);

            var result = await _collection.UpdateOneAsync(x => x.Id == dto.Id, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateStep4Async(Step4EmailDto dto)
        {
            var update = Builders<PersonalInfo>.Update
                .Set(x => x.Email, dto.Email);

            var result = await _collection.UpdateOneAsync(x => x.Id == dto.Id, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateStep5Async(Step5PasscodeDto dto)
        {
            string hashedPasscode = BCrypt.Net.BCrypt.HashPassword(dto.Passcode);

            var update = Builders<PersonalInfo>.Update
                .Set(x => x.Passcode, hashedPasscode)
                .Set(x => x.IsCompleted, dto.IsCompleted);

            var result = await _collection.UpdateOneAsync(x => x.Id == dto.Id, update);
            return result.ModifiedCount > 0;
        }
    }
}
