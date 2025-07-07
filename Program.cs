using TechMobileBE.Services;
using System.Text.Json;
using TechMobileBE.Hubs;

var builder = WebApplication.CreateBuilder(args);

// MongoDB Services
builder.Services.AddSingleton<PersonalInfoService>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<BalanceService>();
builder.Services.AddSingleton<TransactionService>();
builder.Services.AddSingleton<MongoDbService>();

// SignalR
builder.Services.AddSignalR();

// CORS – Mobil istemciler için gerekli!
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true); // Geliştirme ortamı için geçici çözüm. PROD'da sabit origin ver.
    });
});

// Controllers & Swagger
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🔑 CORS middleware'i routing öncesi ve hub öncesi çağrılmalı
app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
