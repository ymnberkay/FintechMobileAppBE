using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TechMobileBE.DTOs;
using TechMobileBE.Models;

namespace TechMobileBE.Services
{
    public class AuthService
    {
        private readonly IMongoCollection<User> _collection;
        private readonly IConfiguration _configuration;

        public AuthService(MongoDbService mongoDbService, IConfiguration configuration)
        {
            _collection = mongoDbService.GetCollection<User>("Auth");
            _configuration = configuration;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterDto dto)
        {
            try
            {
                var existingUser = await _collection.Find(u => u.PhoneNumber == dto.PhoneNumber).FirstOrDefaultAsync();
                if (existingUser != null){
                return new AuthResponse
                {
                    Success = false,
                    Message = "User with this phone number already exists."
                };
            }

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

                var user = new User
                {
                    PhoneNumber = dto.PhoneNumber,
                    PasswordHash = hashedPassword,
                    CreateDate = DateTime.UtcNow
                };

                await _collection.InsertOneAsync(user);

                var token = GenerateJwtToken(user);

                return new AuthResponse
                {
                    Success = true,
                    UserId = user.Id,
                    PhoneNumber = user.PhoneNumber,
                    Token = token,
                    CreateDate = user.CreateDate
                };
            }
            catch (MongoWriteException ex)
            {
                throw new Exception($"Database error: {ex.Message}");
            }
        }

        public async Task<AuthResponse> LoginAsync(string phoneNumber, string password)
        {
            var user = await _collection.Find(u => u.PhoneNumber == phoneNumber).FirstOrDefaultAsync();

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid phone number or password."
                };
            }

            var token = GenerateJwtToken(user);

            return new AuthResponse
            {
                Success = true,
                UserId = user.Id,
                PhoneNumber = user.PhoneNumber,
                Token = token,
                CreateDate = user.CreateDate
            };
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
                new Claim("CreateDate", user.CreateDate.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public string? UserId { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Token { get; set; }
        public DateTime? CreateDate { get; set; }


    }
}

