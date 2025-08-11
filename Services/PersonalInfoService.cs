using MongoDB.Driver;
using TechMobileBE.Models;

namespace TechMobileBE.Services
{
    public class PersonalInfoService
    {
        private readonly IMongoCollection<PersonalInfo> _collection;
        private readonly IMongoCollection<User> _authCollection;

        public PersonalInfoService(MongoDbService mongoDbService)
        {
            _collection = mongoDbService.GetCollection<PersonalInfo>("PersonalInfos");
            _authCollection = mongoDbService.GetCollection<User>("Auth");

        }

        public async Task<string> CreateResidenceAsync(Step1ResidenceDto dto)
        {
            var update = Builders<PersonalInfo>.Update
                .Set(x => x.Id, dto.UserId)  // Set the Id explicitly
                .Set(x => x.Residence, dto.Residence);
            
            var result = await _collection.UpdateOneAsync(
                x => x.Id == dto.UserId,
                update,
                new UpdateOptions { IsUpsert = true }  // Changed to true to create if not exists
            );

            return dto.UserId;
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

            var update = Builders<User>.Update
                .Set(x => x.Passcode, hashedPasscode);

            var userUpdate = Builders<PersonalInfo>.Update
                .Set(x => x.IsCompleted, dto.IsCompleted);

            var result = await _authCollection.UpdateOneAsync(x => x.Id == dto.Id, update);
            var userResult = await _collection.UpdateOneAsync(x => x.Id == dto.Id, userUpdate);
            return result.ModifiedCount > 0;
        }

        public async Task<PersonalInfoDto?> GetPersonalInfoAsync(string userId)
        {
            var projection = Builders<PersonalInfo>.Projection
                .Include(x => x.Id)
                .Include(x => x.Residence)
                .Include(x => x.FullName)
                .Include(x => x.UserName)
                .Include(x => x.DateOfBirth)
                .Include(x => x.AddressLine)
                .Include(x => x.City)
                .Include(x => x.PostCode)
                .Include(x => x.Email);

            var filter = Builders<PersonalInfo>.Filter.Eq(x => x.Id, userId);

            var result = await _collection
                .Find(filter)
                .Project<PersonalInfoDto>(projection)
                .FirstOrDefaultAsync();

            return result;
        }
        // This method retrieves personal information by email
        public async Task<PersonalInfoDto?> GetPersonalInfoByEmailAsync(string email)
        {
            var projection = Builders<PersonalInfo>.Projection
                .Include(x => x.Id)
                .Include(x => x.Residence)
                .Include(x => x.FullName)
                .Include(x => x.UserName)
                .Include(x => x.DateOfBirth)
                .Include(x => x.AddressLine)
                .Include(x => x.City)
                .Include(x => x.PostCode)
                .Include(x => x.Email);

            var filter = Builders<PersonalInfo>.Filter.Eq(x => x.Email, email);

            var result = await _collection
                .Find(filter)
                .Project<PersonalInfoDto>(projection)
                .FirstOrDefaultAsync();

            return result;
        }


    }
}
